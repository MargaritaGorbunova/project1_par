using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

class Program
{
    static void Main()
    {
        // Параметры тестирования
        int totalSeats = 10;
        int customerInterval = 1000;
        int maxWaitingTime = 2;
        int maxEatingTime = 10;
        int numCustomers = 20;

        // Массивы значений параметров для тестирования
        int[] totalSeatsOptions = { 5, 10, 20 };
        int[] customerArrivalIntervals = { 500, 1000, 2000 };
        int[] maxWaitingTimes = { 1, 2, 3 };
        int[] eatingTimeRanges = { 5, 10, 15 };
        int[] numberOfCustomers = { 10, 20, 50 };

        // Исследование влияния количества мест
        foreach (int seats in totalSeatsOptions)
        {
            Console.WriteLine($"Тест с количеством мест: totalSeats = {seats}");
            RunSimulation(seats, customerInterval, maxWaitingTime, maxEatingTime, numCustomers);
        }

        // Исследование влияния интервала прихода покупателей
        foreach (int interval in customerArrivalIntervals)
        {
            Console.WriteLine($"Тест с интервалом прихода покупателей: customerInterval = {interval}");
            RunSimulation(totalSeats, interval, maxWaitingTime, maxEatingTime, numCustomers);
        }

        // Исследование влияния времени ожидания
        foreach (int waitingTime in maxWaitingTimes)
        {
            Console.WriteLine($"Тест с максимальным временем ожидания: maxWaitingTime = {waitingTime}");
            RunSimulation(totalSeats, customerInterval, waitingTime, maxEatingTime, numCustomers);
        }

        // Исследование влияния времени трапезы
        foreach (int eatingTime in eatingTimeRanges)
        {
            Console.WriteLine($"Тест с максимальным временем трапезы: maxEatingTime = {eatingTime}");
            RunSimulation(totalSeats, customerInterval, maxWaitingTime, eatingTime, numCustomers);
        }

        // Исследование влияния количества покупателей
        foreach (int customers in numberOfCustomers)
        {
            Console.WriteLine($"Тест с количеством покупателей: numCustomers = {customers}");
            RunSimulation(totalSeats, customerInterval, maxWaitingTime, maxEatingTime, customers);
        }
    }

    static void RunSimulation(int totalSeats, int customerInterval, int maxWaitingTime, int maxEatingTime, int numCustomers)
    {
        List<bool> seats = Enumerable.Repeat(false, totalSeats).ToList();
        Mutex mutex = new Mutex();
        DateTime start = DateTime.Now;

        for (int i = 0; i < numCustomers; i++)
        {
            int customerId = i + 1;
            Thread customerThread = new Thread(() => Customer(customerId, seats, mutex, maxWaitingTime, maxEatingTime));
            customerThread.Start();
            Thread.Sleep(customerInterval);
        }

        DateTime end = DateTime.Now;
        TimeSpan duration = end - start;
        Console.WriteLine($"Время выполнения: {duration.TotalMilliseconds} мс");
        Console.WriteLine("------------------------------------------------------------");
    }

    static void Customer(int customerId, List<bool> seats, Mutex mutex, int maxWaitingTime, int maxEatingTime)
    {
        Console.WriteLine($"Покупатель {customerId} пришел в столовую");

        bool seated = false;
        int waitTime = 0;

        while (waitTime < maxWaitingTime && !seated)
        {
            mutex.WaitOne();
            try
            {
                int freeSeatIndex = seats.FindIndex(seat => !seat);

                if (freeSeatIndex != -1)
                {
                    seats[freeSeatIndex] = true;
                    seated = true;
                    Console.WriteLine($"Покупатель {customerId} занял место {freeSeatIndex + 1}");

                    int eatingTime = new Random().Next(1, maxEatingTime + 1);
                    new Thread(() => Eat(customerId, freeSeatIndex, eatingTime, seats, mutex)).Start();
                }
                else
                {
                    Console.WriteLine($"Покупатель {customerId} ждет свободного места...");
                }
            }
            finally
            {
                mutex.ReleaseMutex();
            }

            if (!seated)
            {
                Thread.Sleep(2000);
                waitTime++;
            }
        }

        if (!seated)
        {
            Console.WriteLine($"Покупатель {customerId} ушел голодный и обиженный");
        }
    }

    static void Eat(int customerId, int seatIndex, int eatingTime, List<bool> seats, Mutex mutex)
    {
        Thread.Sleep(eatingTime * 1000);
        mutex.WaitOne();
        try
        {
            seats[seatIndex] = false;
            Console.WriteLine($"Покупатель {customerId} закончил есть и освободил место {seatIndex + 1}");
        }
        finally
        {
            mutex.ReleaseMutex();
        }
    }
}
