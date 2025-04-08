using System;
using System.Collections.Concurrent;
using System.Threading;

class Program
{
    static ConcurrentQueue<string> downloadedFiles = new ConcurrentQueue<string>();
    static ConcurrentQueue<string> processedFiles = new ConcurrentQueue<string>();
    static ConcurrentQueue<string> compressedFiles = new ConcurrentQueue<string>();

    static AutoResetEvent downloadEvent = new AutoResetEvent(false);
    static AutoResetEvent processEvent = new AutoResetEvent(false);
    static AutoResetEvent compressEvent = new AutoResetEvent(false);

    static bool downloadFinished = false;
    static bool processFinished = false;
    static bool compressFinished = false;

    static void Main()
    {
        Thread downloader = new Thread(DownloadFiles);
        Thread processor = new Thread(ProcessFiles);
        Thread compressor = new Thread(CompressFiles); // NUEVO HILO
        Thread logger = new Thread(LogFiles);

        downloader.Start();
        processor.Start();
        compressor.Start(); // INICIO DEL NUEVO HILO
        logger.Start();

        downloader.Join();
        processor.Join();
        compressor.Join(); // ESPERA A QUE TERMINE
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
            downloadEvent.Set();
        }

        downloadFinished = true;
        downloadEvent.Set();
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
                processEvent.Set();
            }
            else
            {
                downloadEvent.WaitOne();
            }
        }

        processFinished = true;
        processEvent.Set();
    }

    static void CompressFiles()
    {
        while (!processFinished || !processedFiles.IsEmpty)
        {
            if (processedFiles.TryDequeue(out string file))
            {
                Console.WriteLine($"\n🗜 Comprimiendo {file}...");
                Thread.Sleep(800);
                string compressedFile = file.Replace(".mp4", ".zip");
                Console.WriteLine($"📦 {compressedFile} creado.");

                compressedFiles.Enqueue(compressedFile);
                compressEvent.Set();
            }
            else
            {
                processEvent.WaitOne();
            }
        }

        compressFinished = true;
        compressEvent.Set();
    }

    static void LogFiles()
    {
        while (!compressFinished || !compressedFiles.IsEmpty)
        {
            if (compressedFiles.TryDequeue(out string file))
            {
                Console.WriteLine($"\n📝 Registrando en log: {file}");
                Thread.Sleep(500);
            }
            else
            {
                compressEvent.WaitOne();
            }
        }
    }
}
