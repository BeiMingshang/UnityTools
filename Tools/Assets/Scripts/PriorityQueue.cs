using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 最小堆,更小的数向上移
/// </summary>
/// <typeparam name="T"></typeparam>
public class PriorityQueue<T>
{
    private T[] values;
    /// <summary>
    /// 优先级比较器， 系统写的，就是个委托
    /// </summary>
    private Comparison<T> comparison;
    public int Count { get; private set; }
    /// <summary>
    /// 判断队列是否为空
    /// </summary>

    public bool Empty => Count == 0;
    public int Capacity => values == null ? 0 : values.Length;
    /// <summary>
    /// 二叉树
    /// </summary>
    private const int Arity = 2;
    /// <summary>
    /// 位预算符  每次左移右移只需要移动一位
    /// </summary>
    private const int Log2Arity = 1;

    /// <summary>
    /// 获取父节点的下标
    /// (now-1)/节点个数
    /// 二叉树就是2
    /// </summary>

    public PriorityQueue() {
        values = new T[5];
        comparison = (a, b) => { return a.GetHashCode() > b.GetHashCode() ? 1 : -1; };
    }
    /// <summary>
    /// 带有比较条件的构造器
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
    /// 获取子节点下标 (now*节点个数)+1
    /// 1代表第一个孩子，第二个孩子就是+1+1
    /// </summary>
    private int GetFirstChildIndex(int index) => (index << Log2Arity) + 1;
    
    /// <summary>
    /// 入队 向上更新
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
    /// 交换
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
    /// 向上更新
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
    /// 出队 向下更新
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
    /// 向下更新
    /// </summary>
    /// <returns></returns>
    private void MoveDownUpdate(int index)
    {
        while(GetFirstChildIndex(index) <Count)
        {   int higherIndex= GetFirstChildIndex(index);

            int lc = GetFirstChildIndex(index);
            int rc = GetFirstChildIndex(index) + 1;
            // 检查右孩子是否存在，并且是否比左孩子优先级更高（值更小）
            if (rc < Count && comparison(values[rc], values[lc]) < 0)
            {
                higherIndex = rc; // 更新为右孩子索引
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
    /// 返回最优节点
    /// </summary>
    public T Peek()
    {
        if (Count==0)
            throw new Exception("KPriorityQueue is Empty");
        return values[0];
    }
    /// <summary>
    /// 清除
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
