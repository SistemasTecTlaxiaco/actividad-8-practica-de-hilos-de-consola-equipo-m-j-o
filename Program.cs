using System;
using System.Collections.Generic;
using System.Threading;

class Program
{
    static Queue<string> downloadedFiles = new Queue<string>();
    static object locker = new object();
    static bool downloadFinished = false;

    static void Main(string[] args)
    {
        Thread downloaderThread = new Thread(DownloadFiles);
        Thread processorThread = new Thread(ProcessFiles);

        downloaderThread.Start();
        processorThread.Start();

        downloaderThread.Join();
        processorThread.Join();

        Console.WriteLine("\n✔ Todos los archivos fueron descargados y procesados.");
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

            lock (locker)
            {
                downloadedFiles.Enqueue(file);
                Monitor.Pulse(locker); // Notifica al procesador que hay un archivo nuevo
            }
        }

        lock (locker)
        {
            downloadFinished = true;
            Monitor.Pulse(locker); // Avisa al procesador que ya no habrá más archivos
        }
    }

    static void ProcessFiles()
    {
        while (true)
        {
            string fileToProcess = null;

            lock (locker)
            {
                while (downloadedFiles.Count == 0 && !downloadFinished)
                {
                    Monitor.Wait(locker); // Espera hasta que haya un archivo o termine la descarga
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
            }
        }
    }
}