using NonStandard;
using NonStandard.Data;
using NonStandard.Data.Parse;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemplatedDialog : Dialog {
    public string template, parameters;
}
public class DialogManager : MonoBehaviour
{
    public TextAsset dialogTemplate;
    public TextAsset dialogData;

    void Start()
    {
        Dictionary<string, object> data;
        Tokenizer tok = new Tokenizer();
        CodeConvert.TryParse(dialogData.text, out data, null, tok);
        if (tok.errors.Count > 0) { Debug.LogError(tok.errors.JoinToString("\n")); }
        Debug.Log(tok.DebugPrint());
        Debug.Log(Show.Stringify(data));
        Dialog[] dialogs;
        CodeConvert.TryParse(dialogTemplate.text, out dialogs, data, tok);
        if (tok.errors.Count > 0) { Debug.LogError(tok.errors.JoinToString("\n")); }
        Debug.Log(tok.DebugPrint());
        Debug.Log(Show.Stringify(dialogs));
        // TODO test me
    }
    void Update()
    {
        
    }
}
