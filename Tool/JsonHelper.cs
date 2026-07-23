using System;
using System.Diagnostics;
using System.Text.Json.Nodes;

namespace Foldicon.Tool;

public static class JsonHelper
{
    /// <summary>
    /// 尝试链式获取一个 Json 值
    /// </summary>
    /// <param name="node">根节点</param>
    /// <param name="keys">键</param>
    /// <param name="value">值</param>
    /// <typeparam name="TValueType">目标类型</typeparam>
    /// <returns>是否成功</returns>
    public static bool TryGetJsonValue<TValueType>(JsonNode node, out TValueType? value, params string[] keys)
    {
        try
        {
            var currentNode = node;
            foreach (var parm in keys) currentNode = currentNode![parm];
            value = currentNode!.GetValue<TValueType>();
            return true;
        }
        catch (Exception)
        {
            value = default;
            return false;
        }
    }

    /// <summary>
    /// 尝试链式设置一个 Json 值（若键不存在，则自动创建结构）
    /// </summary>
    /// <param name="node">根节点</param>
    /// <param name="value">值</param>
    /// <param name="keys">键</param>
    /// <returns>是否设置成功</returns>
    public static bool TrySetJsonValue<TValueType>(ref JsonNode node, TValueType value, params string[] keys)
    {
        try
        {
            var current = node;
            for (var i = 0; i < keys.Length; i++)
            {
                var parm = keys[i];
                // 为最后一个 Key 设置 Value
                if (i == keys.Length - 1)
                {
                    current[parm] = JsonValue.Create(value);
                    continue;
                }

                var next = current[parm];
                if (next is not null) current = next;
                else
                {
                    // 若下一个 Key 不存在，则自动创建结构
                    current[parm] = new JsonObject();
                    current = current[parm];
                }
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return false;
        }
    }
}