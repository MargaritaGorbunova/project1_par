using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

class Program
{
    static readonly int TotalSeats = 10;
    static readonly Mutex Mutex = new Mutex();
    static readonly List<bool> Seats = Enumerable.Repeat(false, TotalSeats).ToList(); // false = не занято, true = занято

    static void Main()
    {
        for (int i = 0; ; i++)
        {
            int customerId = i + 1;
            Thread customerThread = new Thread(() => Customer(customerId));
            customerThread.Start();
            Thread.Sleep(1000); // Покупатели приходят каждую секунду
        }
    }

    static void Customer(int customerId)
    {
        Console.WriteLine($"Покупатель {customerId} пришел в столовую");

        bool seated = false; //статус занятости
        int waitTime = 0; //время ожидания

        while (waitTime < 2 && !seated)
        {
            Mutex.WaitOne(); // Блокируем доступ к ресурсам
            try
            {
                int freeSeatIndex = Seats.FindIndex(seat => !seat);

                if (freeSeatIndex != -1)
                {
                    Seats[freeSeatIndex] = true; // Занимаем место
                    seated = true;
                    Console.WriteLine($"Покупатель {customerId} занял место {freeSeatIndex + 1}");

                    int eatingTime = new Random().Next(1, 17); // Время трапезы от 1 до 16 условных единиц
                    new Thread(() => Eat(customerId, freeSeatIndex, eatingTime)).Start();
                }
                else
                {
                    Console.WriteLine($"Покупатель {customerId} ждет свободного места...");
                }
            }
            finally
            {
                Mutex.ReleaseMutex(); // Освобождаем доступ к ресурсам
            }

            if (!seated)
            {
                Thread.Sleep(2000); // Ждем 2 секунды
                waitTime++;
            }
        }

        if (!seated)
        {
            Console.WriteLine($"Покупатель {customerId} ушел голодный и обиженный");
        }
    }

    static void Eat(int customerId, int seatIndex, int eatingTime)
    {
        Thread.Sleep(eatingTime * 1000); // Имитация времени на прием пищи
        Mutex.WaitOne();
        try
        {
            Seats[seatIndex] = false; // Освобождаем место
            Console.WriteLine($"Покупатель {customerId} закончил есть и освободил место {seatIndex + 1}");
        }
        finally
        {
            Mutex.ReleaseMutex();
        }
    }
}
