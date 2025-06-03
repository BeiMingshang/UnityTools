using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class EventCenter : Singleton<EventCenter>
{
    private Dictionary<string, EventHandler> dic=new Dictionary<string, EventHandler>();
    /// <summary>
    /// ����¼�����
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="handler"></param>
    public void AddListener(string eventName, EventHandler handler)
    {
        if (dic.ContainsKey(eventName))
        {
            dic[eventName] += handler;
        }
        else
        {
            dic.Add(eventName, handler);
        }
    }
    /// <summary>
    /// �Ƴ��¼�
    /// </summary>
    /// <param name="eventName"></param>
    public void RemoveListener(string eventName,EventHandler handler)
    {
        if (!dic.ContainsKey(eventName))
        {
            return;
        }else
        {
            dic [eventName] -= handler;
        }
    }
    /// <summary>
    /// �޲��¼�����
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="sender"></param>
    public void TriggerEvent(string eventName, object sender)
    {
        if(dic.ContainsKey(eventName))
        {
            dic[eventName]?.Invoke(sender, EventArgs.Empty);
        }

    }
    /// <summary>
    /// �в��¼�����
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="sender"></param>
    public void TriggerEvent(string eventName, object sender,EventArgs args)
    {
        if (dic.ContainsKey(eventName))
        {
            dic[eventName]?.Invoke(sender, args);
        }

    }
    /// <summary>
    /// ����¼�
    /// </summary>
    public void Clear()
    {
        dic.Clear();
    }
}
/// <summary>
/// ��չ�����࣬��Object�����չ��������ʵ��ģʽ���������̬������this�����Զ����ݣ��������ڵ��÷���
/// EventManager.Instance.TriggerEvent("PlayerDead",this);
/// "PlayerDead"���¼�����this���¼�ӵ���ߣ�Ҳ���ǵ�ǰ����
/// ���ڵĵ��� this.TriggerEvent("PlayerDead");

/// </summary>
public static class EventTriggerExt
{
    public static void TriggerEvent(this object sender, string eventName)
    {
        EventCenter.Instance.TriggerEvent(eventName,sender);
    }
    public static void TriggerEvent(this object sender, string eventName,EventArgs ags)
    {
        EventCenter.Instance.TriggerEvent(eventName, sender,ags);
    }

}
