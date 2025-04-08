using System;
using System.Collections.Generic;
using System.Threading;

class Program
{
    static Queue<string> downloadedFiles = new Queue<string>();
    static Queue<string> processedFiles = new Queue<string>();
    static object downloadLocker = new object();
    static object processLocker = new object();

    static bool downloadFinished = false;
    static bool processFinished = false;

    static void Main(string[] args)
    {
        Thread downloaderThread = new Thread(DownloadFiles);
        Thread processorThread = new Thread(ProcessFiles);
        Thread loggerThread = new Thread(LogProcessedFiles);

        downloaderThread.Start();
        processorThread.Start();
        loggerThread.Start();

        downloaderThread.Join();
        processorThread.Join();
        loggerThread.Join();

        Console.WriteLine("\n✔ Todos los archivos fueron descargados, procesados y registrados.");
    }

    static void DownloadFiles()
    {
        string[] files = { "Archivo1.mp4", "Archivo2.mp4", "Archivo3.mp4", "Archivo4.mp4" };

        foreach (var file in files)
        {
            Console.WriteLine($"\n⬇ Descargando {file}...");
            for (int i = 0; i <= 100; i += 20)
            {
                Thread.Sleep(200); // Simula descarga
                Console.WriteLine($"  Progreso {file}: {i}%");
            }

            lock (downloadLocker)
            {
                downloadedFiles.Enqueue(file);
                Monitor.Pulse(downloadLocker); // Notifica al procesador que hay un archivo nuevo
            }
        }

        lock (downloadLocker)
        {
            downloadFinished = true;
            Monitor.Pulse(downloadLocker); // Avisa al procesador que ya no habrá más archivos
        }
    }

    static void ProcessFiles()
    {
        while (true)
        {
            string fileToProcess = null;

            lock (downloadLocker)
            {
                while (downloadedFiles.Count == 0 && !downloadFinished)
                {
                    Monitor.Wait(downloadLocker);
                }

                if (downloadedFiles.Count > 0)
                {
                    fileToProcess = downloadedFiles.Dequeue();
                }
                else if (downloadFinished)
                {
                    break;
                }
            }

            if (fileToProcess != null)
            {
                Console.WriteLine($"\n⚙ Procesando {fileToProcess}...");
                Thread.Sleep(1000); // Simula procesamiento
                Console.WriteLine($"✅ {fileToProcess} procesado.");

                lock (processLocker)
                {
                    processedFiles.Enqueue(fileToProcess);
                    Monitor.Pulse(processLocker); // Notifica al logger
                }
            }
        }

        lock (processLocker)
        {
            processFinished = true;
            Monitor.Pulse(processLocker); // Avisa al logger que ya no habrá más archivos
        }
    }

    static void LogProcessedFiles()
    {
        while (true)
        {
            string processedFile = null;

            lock (processLocker)
            {
                while (processedFiles.Count == 0 && !processFinished)
                {
                    Monitor.Wait(processLocker);
                }

                if (processedFiles.Count > 0)
                {
                    processedFile = processedFiles.Dequeue();
                }
                else if (processFinished)
                {
                    break;
                }
            }

            if (processedFile != null)
            {
                Console.WriteLine($"\n📝 Registrando en log: {processedFile}");
                Thread.Sleep(500); // Simula escritura en log
            }
        }
    }
}