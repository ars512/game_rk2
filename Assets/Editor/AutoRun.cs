using System;
using System.IO;
using UnityEditor;
using UnityEngine;

// Вспомогательный хук для CI/скриптов: если в Temp лежит файл run_request.txt с именем
// статического метода («Класс.Метод»), открытый редактор выполняет его и пишет run_result.txt.
[InitializeOnLoad]
public static class AutoRun
{
    const string Req = "Temp/run_request.txt", Res = "Temp/run_result.txt";

    static AutoRun() { EditorApplication.update += Poll; }

    static void Poll()
    {
        if (!File.Exists(Req) || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        string call = File.ReadAllText(Req).Trim();
        var parts = call.Split('.');
        var type = Type.GetType(parts[0]);
        if (type == null) { AssetDatabase.Refresh(); return; } // скрипт ещё не скомпилирован
        File.Delete(Req);
        try
        {
            type.GetMethod(parts[1]).Invoke(null, null);
            File.WriteAllText(Res, "OK " + call);
        }
        catch (Exception e)
        {
            File.WriteAllText(Res, "FAIL " + call + "\n" + e);
            Debug.LogException(e);
        }
    }
}
