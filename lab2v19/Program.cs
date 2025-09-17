using System;
using System.Text;

class Queue
{
    private int[] elements; 
    private int count;      

    public Queue(int capacity = 10)
    {
        elements = new int[capacity];
        count = 0;
    }

    
    public int Count => count;

   
    public int this[int index]
    {
        get
        {
            if (index < 0 || index >= count)
                throw new IndexOutOfRangeException("Некоректний індекс");
            return elements[index];
        }
        set
        {
            if (index < 0 || index >= count)
                throw new IndexOutOfRangeException("Некоректний індекс");
            elements[index] = value;
        }
    }

    public static Queue operator +(Queue q, int value)
    {
        if (q.count == q.elements.Length)
        {
            Array.Resize(ref q.elements, q.elements.Length * 2);
        }
        q.elements[q.count++] = value;
        return q;
    }

    public static Queue operator -(Queue q)
    {
        if (q.count == 0)
            throw new InvalidOperationException("Черга порожня!");

        for (int i = 0; i < q.count - 1; i++)
        {
            q.elements[i] = q.elements[i + 1];
        }
        q.count--;
        return q;
    }

    public void Print()
    {
        for (int i = 0; i < count; i++)
        {
            Console.Write(elements[i] + " ");
        }
        Console.WriteLine();
    }
}

class Program
{
    static void Main()
    {
        Console.InputEncoding = Console.OutputEncoding = Encoding.Unicode;

        Queue q = new Queue();

        q = q + 10;
        q = q + 20;
        q = q + 30;

        Console.WriteLine("Черга після додавання:");
        q.Print();

        q = -q;
        Console.WriteLine("Черга після видалення першого елемента:");
        q.Print();

        Console.WriteLine("Елемент з індексом [0]: " + q[0]);
    }
}
