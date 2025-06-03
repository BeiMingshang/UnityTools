using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class EventCenter : Singleton<EventCenter>
{
    private Dictionary<string, EventHandler> dic=new Dictionary<string, EventHandler>();
    /// <summary>
    /// 添加事件函数
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
    /// 移除事件
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
    /// 无参事件触发
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
    /// 有参事件触发
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
    /// 清空事件
    /// </summary>
    public void Clear()
    {
        dic.Clear();
    }
}
/// <summary>
/// 拓展方法类，给Object添加拓展方法（以实例模式调用这个静态方法（this参数自动传递）），便于调用方法
/// EventManager.Instance.TriggerEvent("PlayerDead",this);
/// "PlayerDead"是事件名，this是事件拥有者，也就是当前的类
/// 现在的调用 this.TriggerEvent("PlayerDead");

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
