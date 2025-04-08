using System;
using System.Collections.Concurrent;
using System.Threading;

class Program
{
    static ConcurrentQueue<string> downloadedFiles = new ConcurrentQueue<string>();
    static ConcurrentQueue<string> processedFiles = new ConcurrentQueue<string>();

    static AutoResetEvent downloadEvent = new AutoResetEvent(false);
    static AutoResetEvent processEvent = new AutoResetEvent(false);

    static bool downloadFinished = false;
    static bool processFinished = false;

    static void Main()
    {
        Thread downloader = new Thread(DownloadFiles);
        Thread processor = new Thread(ProcessFiles);
        Thread logger = new Thread(LogFiles);

        downloader.Start();
        processor.Start();
        logger.Start();

        downloader.Join();
        processor.Join();
        logger.Join();

        Console.WriteLine("\n✔ Todo finalizado correctamente.");
    }

    static void DownloadFiles()
    {
        string[] files = { "Archivo1.mp4", "Archivo2.mp4", "Archivo3.mp4", "Archivo4.mp4" };

        foreach (var file in files)
        {
            Console.WriteLine($"\n⬇ Descargando {file}...");
            for (int i = 0; i <= 100; i += 25)
            {
                Thread.Sleep(200);
                Console.WriteLine($"  Progreso {file}: {i}%");
            }

            downloadedFiles.Enqueue(file);
            downloadEvent.Set(); // Señala al procesador que hay un archivo
        }

        downloadFinished = true;
        downloadEvent.Set(); // Despierta al procesador por si está esperando
    }

    static void ProcessFiles()
    {
        while (!downloadFinished || !downloadedFiles.IsEmpty)
        {
            if (downloadedFiles.TryDequeue(out string file))
            {
                Console.WriteLine($"\n⚙ Procesando {file}...");
                Thread.Sleep(1000);
                Console.WriteLine($"✅ {file} procesado.");

                processedFiles.Enqueue(file);
                processEvent.Set(); // Señala al logger
            }
            else
            {
                downloadEvent.WaitOne();
            }
        }

        processFinished = true;
        processEvent.Set(); // Despierta al logger por si está esperando
    }

    static void LogFiles()
    {
        while (!processFinished || !processedFiles.IsEmpty)
        {
            if (processedFiles.TryDequeue(out string file))
            {
                Console.WriteLine($"\n📝 Registrando en log: {file}");
                Thread.Sleep(500);
            }
            else
            {
                processEvent.WaitOne();
            }
        }
    }
}
