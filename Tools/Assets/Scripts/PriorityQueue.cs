using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��С��,��С����������
/// </summary>
/// <typeparam name="T"></typeparam>
public class PriorityQueue<T>
{
    private T[] values;
    /// <summary>
    /// ���ȼ��Ƚ����� ϵͳд�ģ����Ǹ�ί��
    /// </summary>
    private Comparison<T> comparison;
    public int Count { get; private set; }
    /// <summary>
    /// �ж϶����Ƿ�Ϊ��
    /// </summary>

    public bool Empty => Count == 0;
    public int Capacity => values == null ? 0 : values.Length;
    /// <summary>
    /// ������
    /// </summary>
    private const int Arity = 2;
    /// <summary>
    /// λԤ���  ÿ����������ֻ��Ҫ�ƶ�һλ
    /// </summary>
    private const int Log2Arity = 1;

    /// <summary>
    /// ��ȡ���ڵ���±�
    /// (now-1)/�ڵ����
    /// ����������2
    /// </summary>

    public PriorityQueue() {
        values = new T[5];
        comparison = (a, b) => { return a.GetHashCode() > b.GetHashCode() ? 1 : -1; };
    }
    /// <summary>
    /// ���бȽ������Ĺ�����
    /// </summary>
    /// <param name="comparison"></param>
    public PriorityQueue(Comparison<T> comparison) : this()
    {
        this.comparison = comparison;
    }
    private void Expansion()
    {
        T[] _values = new T[Capacity * 2];

        for (int i = 0; i < Count; i++)
        {
            _values[i] = values[i];
        }

        values = _values;
    }
    private int GetParentIndex(int index) => (index - 1) >> Log2Arity;


    /// <summary>
    /// ��ȡ�ӽڵ��±� (now*�ڵ����)+1
    /// 1�����һ�����ӣ��ڶ������Ӿ���+1+1
    /// </summary>
    private int GetFirstChildIndex(int index) => (index << Log2Arity) + 1;
    
    /// <summary>
    /// ��� ���ϸ���
    /// </summary>

    public void  Enqueue(T value)
    {
        if (Count >= Capacity)
        {
            Expansion();
        } 
        values[Count] = value;
        MoveUpUpdate(Count);
        Count++;

    }
    /// <summary>
    /// ����
    /// </summary>
    private void Swap(int i, int j)
    {
        if (i == j)
            return;

        T temp = values[i];
        values[i] = values[j];
        values[j] = temp;
    }
    /// <summary>
    /// ���ϸ���
    /// </summary>
    private void MoveUpUpdate(int index)
    {
        while(index>0)
        {
            int p= GetParentIndex(index);
            if (comparison(values[index], values[p]) > 0)
                break;
            else
            {
                Swap(index, p);
                index = p;
            }
        }
    }

    /// <summary>
    /// ���� ���¸���
    /// </summary>
    public T Dequeue() 
    {
        T value = values[0];
        Swap(0, Count-1);
        Count--;
        MoveDownUpdate(0);
        return value;
    }
    /// <summary>
    /// ���¸���
    /// </summary>
    /// <returns></returns>
    private void MoveDownUpdate(int index)
    {
        while(GetFirstChildIndex(index) <Count)
        {   int higherIndex= GetFirstChildIndex(index);

            int lc = GetFirstChildIndex(index);
            int rc = GetFirstChildIndex(index) + 1;
            // ����Һ����Ƿ���ڣ������Ƿ���������ȼ����ߣ�ֵ��С��
            if (rc < Count && comparison(values[rc], values[lc]) < 0)
            {
                higherIndex = rc; // ����Ϊ�Һ�������
            }

            if (comparison(values[index], values[higherIndex]) > 0)
            {
                Swap(index, higherIndex);
                index= higherIndex;
            }
            else break;
            
        }
        
    }
    /// <summary>
    /// �������Žڵ�
    /// </summary>
    public T Peek()
    {
        if (Count==0)
            throw new Exception("KPriorityQueue is Empty");
        return values[0];
    }
    /// <summary>
    /// ���
    /// </summary>
    public void Clear()
    {
        if (Count > 0)
        {
            Array.Clear(values, 0, Count);
            Count = 0;
        }
    }

}
