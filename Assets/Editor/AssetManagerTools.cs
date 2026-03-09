using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// AssetManagerTools — funziona sia su GameObject in Hierarchy che su asset nel Project.
/// Menu: Tools > Asset Manager > ...
/// </summary>
public static class AssetManagerTools
{
    // ─────────────────────────────────────────────
    //  TOGGLE ATTIVO / DISATTIVO  (Ctrl+Shift+A)
    // ─────────────────────────────────────────────

    [MenuItem("Tools/Asset Manager/Attiva-Disattiva GameObject %#a", false, 0)]
    static void ToggleActive()
    {
        var gos = Selection.gameObjects;
        if (gos.Length == 0) return;

        // Usa lo stato del primo oggetto come riferimento per invertire tutti
        bool newState = !gos[0].activeSelf;
        Undo.RecordObjects(gos.Cast<Object>().ToArray(), "Toggle Active");
        foreach (var go in gos)
            go.SetActive(newState);
    }

    [MenuItem("Tools/Asset Manager/Attiva-Disattiva GameObject %#a", true)]
    static bool ToggleActive_Validate() => Selection.gameObjects.Length > 0;

    // ─────────────────────────────────────────────
    //  RINOMINA N OGGETTI / ASSET SELEZIONATI
    // ─────────────────────────────────────────────

    [MenuItem("Tools/Asset Manager/Rinomina selezionati...", false, 10)]
    static void RinominaSelezionati()
    {
        if (Selection.gameObjects.Length > 0 && !HasProjectAssets())
        {
            // Siamo in Hierarchy
            RenameGameObjectWindow.Open(Selection.gameObjects.ToList());
        }
        else
        {
            var assets = GetSelectedAssets();
            if (assets.Count == 0)
            {
                EditorUtility.DisplayDialog("Nessuna selezione",
                    "Seleziona almeno un GameObject nella Hierarchy oppure un asset nel Project.", "OK");
                return;
            }
            RenameAssetWindow.Open(assets);
        }
    }

    [MenuItem("Tools/Asset Manager/Rinomina selezionati...", true)]
    static bool RinominaSelezionati_Validate() => Selection.objects.Length > 0;

    // ─────────────────────────────────────────────
    //  ORDINA IN ORDINE ALFABETICO
    // ─────────────────────────────────────────────

    [MenuItem("Tools/Asset Manager/Ordina in ordine alfabetico", false, 30)]
    static void OrdinaAlfabetico()
    {
        if (Selection.gameObjects.Length > 0 && !HasProjectAssets())
        {
            var gos = Selection.gameObjects.OrderBy(g => g.name).ToList();
            int digits = gos.Count.ToString().Length;
            bool confirm = EditorUtility.DisplayDialog("Ordina in ordine alfabetico",
                $"Verranno aggiunti prefissi numerici a {gos.Count} GameObject.\nContinuare?", "Sì", "Annulla");
            if (!confirm) return;

            Undo.RecordObjects(gos.Cast<Object>().ToArray(), "Ordina alfabetico");
            for (int i = 0; i < gos.Count; i++)
            {
                string baseName = StripLeadingIndex(gos[i].name);
                gos[i].name = $"{(i + 1).ToString().PadLeft(digits, '0')}_{baseName}";
            }
        }
        else
        {
            var assets = GetSelectedAssets();
            if (assets.Count < 2) { EditorUtility.DisplayDialog("Info", "Seleziona almeno 2 asset.", "OK"); return; }

            var sorted = assets.OrderBy(p => Path.GetFileNameWithoutExtension(p)).ToList();
            int digits = sorted.Count.ToString().Length;
            bool confirm = EditorUtility.DisplayDialog("Ordina in ordine alfabetico",
                $"Verranno aggiunti prefissi numerici a {sorted.Count} asset.\nContinuare?", "Sì", "Annulla");
            if (!confirm) return;

            AssetDatabase.StartAssetEditing();
            try
            {
                for (int i = 0; i < sorted.Count; i++)
                {
                    string baseName = StripLeadingIndex(Path.GetFileNameWithoutExtension(sorted[i]));
                    string newName = $"{(i + 1).ToString().PadLeft(digits, '0')}_{baseName}";
                    string err = AssetDatabase.RenameAsset(sorted[i], newName);
                    if (!string.IsNullOrEmpty(err)) Debug.LogWarning($"[AssetManager] {sorted[i]}: {err}");
                }
            }
            finally { AssetDatabase.StopAssetEditing(); AssetDatabase.Refresh(); }
        }
    }

    [MenuItem("Tools/Asset Manager/Ordina in ordine alfabetico", true)]
    static bool OrdinaAlfabetico_Validate() => Selection.objects.Length >= 2;

    // ─────────────────────────────────────────────
    //  RIMUOVI PREFISSO NUMERICO
    // ─────────────────────────────────────────────

    [MenuItem("Tools/Asset Manager/Rimuovi prefisso numerico", false, 31)]
    static void RimuoviPrefisso()
    {
        if (Selection.gameObjects.Length > 0 && !HasProjectAssets())
        {
            var gos = Selection.gameObjects;
            Undo.RecordObjects(gos.Cast<Object>().ToArray(), "Rimuovi prefisso");
            foreach (var go in gos)
                go.name = StripLeadingIndex(go.name);
        }
        else
        {
            var assets = GetSelectedAssets();
            if (assets.Count == 0) return;
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var path in assets)
                {
                    string baseName = StripLeadingIndex(Path.GetFileNameWithoutExtension(path));
                    string err = AssetDatabase.RenameAsset(path, baseName);
                    if (!string.IsNullOrEmpty(err)) Debug.LogWarning($"[AssetManager] {path}: {err}");
                }
            }
            finally { AssetDatabase.StopAssetEditing(); AssetDatabase.Refresh(); }
        }
    }

    [MenuItem("Tools/Asset Manager/Rimuovi prefisso numerico", true)]
    static bool RimuoviPrefisso_Validate() => Selection.objects.Length > 0;

    // ─────────────────────────────────────────────
    //  NUMERA OGGETTI SELEZIONATI
    // ─────────────────────────────────────────────

    [MenuItem("Tools/Asset Manager/Numera selezionati...", false, 20)]
    static void NumeraSelezionati()
    {
        if (Selection.gameObjects.Length > 0 && !HasProjectAssets())
            NumberingWindow.Open(Selection.gameObjects.ToList(), null);
        else
        {
            var assets = GetSelectedAssets();
            if (assets.Count == 0) { EditorUtility.DisplayDialog("Nessuna selezione", "Seleziona almeno un oggetto.", "OK"); return; }
            NumberingWindow.Open(null, assets);
        }
    }

    [MenuItem("Tools/Asset Manager/Numera selezionati...", true)]
    static bool NumeraSelezionati_Validate() => Selection.objects.Length > 0;

    // ─────────────────────────────────────────────
    //  TROVA E SOSTITUISCI NEL NOME
    // ─────────────────────────────────────────────

    [MenuItem("Tools/Asset Manager/Trova e sostituisci nel nome...", false, 50)]
    static void TrovaSostituisci()
    {
        var names = GetSelectedNames(out var goList, out var assetList);
        if (names.Count == 0) { EditorUtility.DisplayDialog("Nessuna selezione", "Seleziona almeno un oggetto.", "OK"); return; }
        FindReplaceWindow.Open(names, goList, assetList);
    }

    [MenuItem("Tools/Asset Manager/Trova e sostituisci nel nome...", true)]
    static bool TrovaSostituisci_Validate() => Selection.objects.Length > 0;

    // ─────────────────────────────────────────────
    //  CONVERTI NAMING CONVENTION
    // ─────────────────────────────────────────────

    [MenuItem("Tools/Asset Manager/Converti nomi/In snake_case", false, 70)]
    static void ToSnakeCase() => ConvertAllNames(s => Regex.Replace(Regex.Replace(s, @"([a-z])([A-Z])", "$1_$2"), @"[\s\-]+", "_").ToLower());
    [MenuItem("Tools/Asset Manager/Converti nomi/In snake_case", true)]
    static bool ToSnakeCase_V() => Selection.objects.Length > 0;

    [MenuItem("Tools/Asset Manager/Converti nomi/In PascalCase", false, 71)]
    static void ToPascalCase() => ConvertAllNames(s => string.Concat(Regex.Split(s, @"[\s_\-]+").Select(w => w.Length > 0 ? char.ToUpper(w[0]) + w.Substring(1) : "")));
    [MenuItem("Tools/Asset Manager/Converti nomi/In PascalCase", true)]
    static bool ToPascalCase_V() => Selection.objects.Length > 0;

    [MenuItem("Tools/Asset Manager/Converti nomi/In kebab-case", false, 72)]
    static void ToKebabCase() => ConvertAllNames(s => Regex.Replace(Regex.Replace(s, @"([a-z])([A-Z])", "$1-$2"), @"[\s_]+", "-").ToLower());
    [MenuItem("Tools/Asset Manager/Converti nomi/In kebab-case", true)]
    static bool ToKebabCase_V() => Selection.objects.Length > 0;

    // ─────────────────────────────────────────────
    //  AGGIUNGI PREFISSO / SUFFISSO
    // ─────────────────────────────────────────────

    [MenuItem("Tools/Asset Manager/Aggiungi prefisso o suffisso...", false, 90)]
    static void AggiungiPrefissoSuffisso()
    {
        var names = GetSelectedNames(out var goList, out var assetList);
        if (names.Count == 0) { EditorUtility.DisplayDialog("Nessuna selezione", "Seleziona almeno un oggetto.", "OK"); return; }
        PrefixSuffixWindow.Open(names, goList, assetList);
    }
    [MenuItem("Tools/Asset Manager/Aggiungi prefisso o suffisso...", true)]
    static bool AggiungiPrefissoSuffisso_V() => Selection.objects.Length > 0;

    // ══════════════════════════════════════════════════════════════
    //  HELPERS
    // ══════════════════════════════════════════════════════════════

    static bool HasProjectAssets() =>
        Selection.objects.Any(o => !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(o)));

    static List<string> GetSelectedAssets() =>
        Selection.objects
            .Select(AssetDatabase.GetAssetPath)
            .Where(p => !string.IsNullOrEmpty(p) && !AssetDatabase.IsValidFolder(p))
            .ToList();

    static List<string> GetSelectedNames(out List<GameObject> goList, out List<string> assetList)
    {
        goList = new List<GameObject>();
        assetList = new List<string>();

        if (Selection.gameObjects.Length > 0 && !HasProjectAssets())
        {
            goList = Selection.gameObjects.ToList();
            return goList.Select(g => g.name).ToList();
        }
        else
        {
            assetList = GetSelectedAssets();
            return assetList.Select(p => Path.GetFileNameWithoutExtension(p)).ToList();
        }
    }

    static string StripLeadingIndex(string name) =>
        Regex.Replace(name, @"^\d+[\s_\-]+", "");

    static void ConvertAllNames(System.Func<string, string> converter)
    {
        if (Selection.gameObjects.Length > 0 && !HasProjectAssets())
        {
            var gos = Selection.gameObjects;
            Undo.RecordObjects(gos.Cast<Object>().ToArray(), "Converti nomi");
            foreach (var go in gos)
            {
                string newName = converter(go.name);
                if (newName != go.name) go.name = newName;
            }
        }
        else
        {
            var assets = GetSelectedAssets();
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var path in assets)
                {
                    string oldName = Path.GetFileNameWithoutExtension(path);
                    string newName = converter(oldName);
                    if (newName != oldName)
                    {
                        string err = AssetDatabase.RenameAsset(path, newName);
                        if (!string.IsNullOrEmpty(err)) Debug.LogWarning($"[AssetManager] {path}: {err}");
                    }
                }
            }
            finally { AssetDatabase.StopAssetEditing(); AssetDatabase.Refresh(); }
        }
    }

    // Metodo pubblico condiviso dalle finestre per applicare il rename
    public static void ApplyRename(List<GameObject> goList, List<string> assetList,
        System.Func<string, int, string> nameBuilder)
    {
        if (goList != null && goList.Count > 0)
        {
            Undo.RecordObjects(goList.Cast<Object>().ToArray(), "Rinomina");
            for (int i = 0; i < goList.Count; i++)
                goList[i].name = nameBuilder(goList[i].name, i);
        }
        else if (assetList != null && assetList.Count > 0)
        {
            AssetDatabase.StartAssetEditing();
            try
            {
                for (int i = 0; i < assetList.Count; i++)
                {
                    string oldName = Path.GetFileNameWithoutExtension(assetList[i]);
                    string newName = nameBuilder(oldName, i);
                    if (newName != oldName)
                    {
                        string err = AssetDatabase.RenameAsset(assetList[i], newName);
                        if (!string.IsNullOrEmpty(err)) Debug.LogWarning($"[AssetManager] {assetList[i]}: {err}");
                    }
                }
            }
            finally { AssetDatabase.StopAssetEditing(); AssetDatabase.Refresh(); }
        }
    }
}

// ══════════════════════════════════════════════════════════════
//  FINESTRA: RINOMINA GAMEOBJECT (Hierarchy)
// ══════════════════════════════════════════════════════════════
public class RenameGameObjectWindow : EditorWindow
{
    List<GameObject> _gos;
    string _pattern = "{name}";
    int _startIndex = 1;
    bool _preview = true;
    Vector2 _scroll;

    public static void Open(List<GameObject> gos)
    {
        var w = GetWindow<RenameGameObjectWindow>("Rinomina GameObject");
        w._gos = gos;
        w.minSize = new Vector2(450, 380);
    }

    void OnGUI()
    {
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Pattern rinomina", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Variabili:\n  {name}   = nome originale\n  {index}  = numero progressivo", MessageType.Info);

        _pattern = EditorGUILayout.TextField("Pattern", _pattern);
        _startIndex = EditorGUILayout.IntField("Indice iniziale", _startIndex);
        _preview = EditorGUILayout.Toggle("Anteprima", _preview);

        if (_preview && _gos != null)
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Anteprima:", EditorStyles.boldLabel);
            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Height(160));
            int digits = _gos.Count.ToString().Length;
            for (int i = 0; i < _gos.Count; i++)
            {
                string newName = BuildName(_pattern, _gos[i].name, i + _startIndex, digits);
                EditorGUILayout.LabelField($"  {_gos[i].name}  →  {newName}");
            }
            EditorGUILayout.EndScrollView();
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Rinomina", GUILayout.Height(30))) { Apply(); Close(); }
        if (GUILayout.Button("Annulla", GUILayout.Height(30))) Close();
        EditorGUILayout.EndHorizontal();
    }

    void Apply()
    {
        int digits = _gos.Count.ToString().Length;
        Undo.RecordObjects(_gos.Cast<Object>().ToArray(), "Rinomina GameObject");
        for (int i = 0; i < _gos.Count; i++)
            _gos[i].name = BuildName(_pattern, _gos[i].name, i + _startIndex, digits);
    }

    static string BuildName(string pattern, string name, int index, int digits) =>
        pattern.Replace("{name}", name).Replace("{index}", index.ToString().PadLeft(digits, '0'));
}

// ══════════════════════════════════════════════════════════════
//  FINESTRA: RINOMINA ASSET (Project)
// ══════════════════════════════════════════════════════════════
public class RenameAssetWindow : EditorWindow
{
    List<string> _paths;
    string _pattern = "{name}";
    int _startIndex = 1;
    bool _preview = true;
    Vector2 _scroll;

    public static void Open(List<string> paths)
    {
        var w = GetWindow<RenameAssetWindow>("Rinomina Asset");
        w._paths = paths;
        w.minSize = new Vector2(450, 380);
    }

    void OnGUI()
    {
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Pattern rinomina", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Variabili:\n  {name}   = nome originale\n  {index}  = numero progressivo\n  {ext}    = estensione", MessageType.Info);

        _pattern = EditorGUILayout.TextField("Pattern", _pattern);
        _startIndex = EditorGUILayout.IntField("Indice iniziale", _startIndex);
        _preview = EditorGUILayout.Toggle("Anteprima", _preview);

        if (_preview && _paths != null)
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Anteprima:", EditorStyles.boldLabel);
            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Height(160));
            int digits = _paths.Count.ToString().Length;
            for (int i = 0; i < _paths.Count; i++)
            {
                string orig = Path.GetFileNameWithoutExtension(_paths[i]);
                string ext = Path.GetExtension(_paths[i]).TrimStart('.');
                string newName = BuildName(_pattern, orig, i + _startIndex, digits, ext);
                EditorGUILayout.LabelField($"  {orig}  →  {newName}");
            }
            EditorGUILayout.EndScrollView();
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Rinomina", GUILayout.Height(30))) { Apply(); Close(); }
        if (GUILayout.Button("Annulla", GUILayout.Height(30))) Close();
        EditorGUILayout.EndHorizontal();
    }

    void Apply()
    {
        int digits = _paths.Count.ToString().Length;
        AssetDatabase.StartAssetEditing();
        try
        {
            for (int i = 0; i < _paths.Count; i++)
            {
                string orig = Path.GetFileNameWithoutExtension(_paths[i]);
                string ext = Path.GetExtension(_paths[i]).TrimStart('.');
                string newName = BuildName(_pattern, orig, i + _startIndex, digits, ext);
                string err = AssetDatabase.RenameAsset(_paths[i], newName);
                if (!string.IsNullOrEmpty(err)) Debug.LogWarning($"[AssetManager] {_paths[i]}: {err}");
            }
        }
        finally { AssetDatabase.StopAssetEditing(); AssetDatabase.Refresh(); }
    }

    static string BuildName(string pattern, string name, int index, int digits, string ext) =>
        pattern.Replace("{name}", name)
               .Replace("{index}", index.ToString().PadLeft(digits, '0'))
               .Replace("{ext}", ext);
}

// ══════════════════════════════════════════════════════════════
//  FINESTRA: TROVA E SOSTITUISCI
// ══════════════════════════════════════════════════════════════
public class FindReplaceWindow : EditorWindow
{
    List<string> _names;
    List<GameObject> _gos;
    List<string> _assets;
    string _find = "", _replace = "";
    bool _caseSensitive = true, _useRegex = false;
    Vector2 _scroll;

    public static void Open(List<string> names, List<GameObject> gos, List<string> assets)
    {
        var w = GetWindow<FindReplaceWindow>("Trova e Sostituisci");
        w._names = names; w._gos = gos; w._assets = assets;
        w.minSize = new Vector2(400, 280);
    }

    void OnGUI()
    {
        EditorGUILayout.Space(6);
        _find = EditorGUILayout.TextField("Cerca", _find);
        _replace = EditorGUILayout.TextField("Sostituisci con", _replace);
        _caseSensitive = EditorGUILayout.Toggle("Case sensitive", _caseSensitive);
        _useRegex = EditorGUILayout.Toggle("Usa Regex", _useRegex);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Anteprima:", EditorStyles.boldLabel);
        _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Height(120));
        if (_names != null)
            foreach (var n in _names)
            {
                string next = DoReplace(n);
                if (next != n) EditorGUILayout.LabelField($"  {n}  →  {next}");
            }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(8);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Applica", GUILayout.Height(30))) { Apply(); Close(); }
        if (GUILayout.Button("Annulla", GUILayout.Height(30))) Close();
        EditorGUILayout.EndHorizontal();
    }

    string DoReplace(string input)
    {
        if (string.IsNullOrEmpty(_find)) return input;
        if (_useRegex) return Regex.Replace(input, _find, _replace, _caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
        return input.Replace(_find, _replace, _caseSensitive ? System.StringComparison.Ordinal : System.StringComparison.OrdinalIgnoreCase);
    }

    void Apply() => AssetManagerTools.ApplyRename(_gos, _assets, (name, _) => DoReplace(name));
}

// ══════════════════════════════════════════════════════════════
//  FINESTRA: PREFISSO / SUFFISSO
// ══════════════════════════════════════════════════════════════
public class PrefixSuffixWindow : EditorWindow
{
    List<string> _names;
    List<GameObject> _gos;
    List<string> _assets;
    string _prefix = "", _suffix = "";
    Vector2 _scroll;

    public static void Open(List<string> names, List<GameObject> gos, List<string> assets)
    {
        var w = GetWindow<PrefixSuffixWindow>("Prefisso / Suffisso");
        w._names = names; w._gos = gos; w._assets = assets;
        w.minSize = new Vector2(380, 260);
    }

    void OnGUI()
    {
        EditorGUILayout.Space(6);
        _prefix = EditorGUILayout.TextField("Prefisso", _prefix);
        _suffix = EditorGUILayout.TextField("Suffisso", _suffix);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Anteprima:", EditorStyles.boldLabel);
        _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Height(120));
        if (_names != null)
            foreach (var n in _names)
                EditorGUILayout.LabelField($"  {n}  →  {_prefix}{n}{_suffix}");
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(8);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Applica", GUILayout.Height(30))) { Apply(); Close(); }
        if (GUILayout.Button("Annulla", GUILayout.Height(30))) Close();
        EditorGUILayout.EndHorizontal();
    }

    void Apply() => AssetManagerTools.ApplyRename(_gos, _assets, (name, _) => _prefix + name + _suffix);
}

// ══════════════════════════════════════════════════════════════
//  FINESTRA: NUMERA OGGETTI
// ══════════════════════════════════════════════════════════════
public class NumberingWindow : EditorWindow
{
    List<GameObject> _gos;
    List<string> _assets;

    // Opzioni
    string _separator = "_";          // es. "_", " ", "-", "."
    int _startIndex = 1;
    bool _padZero = true;             // 01, 02 ... oppure 1, 2 ...
    enum Position { Suffix, Prefix }
    Position _position = Position.Suffix;
    bool _stripExisting = true;       // rimuove eventuali numeri già presenti
    Vector2 _scroll;

    public static void Open(List<GameObject> gos, List<string> assets)
    {
        var w = GetWindow<NumberingWindow>("Numera Oggetti");
        w._gos = gos;
        w._assets = assets;
        w.minSize = new Vector2(420, 360);
    }

    List<string> SourceNames()
    {
        if (_gos != null && _gos.Count > 0)
            return _gos.Select(g => g.name).ToList();
        if (_assets != null && _assets.Count > 0)
            return _assets.Select(p => Path.GetFileNameWithoutExtension(p)).ToList();
        return new List<string>();
    }

    string BuildNumbered(string name, int index, int total)
    {
        if (_stripExisting)
        {
            // Rimuove sia suffissi che prefissi numerici già presenti
            name = Regex.Replace(name, @"^\d+[\s_\-\.]+", "");       // prefisso
            name = Regex.Replace(name, @"[\s_\-\.]+\d+$", "");       // suffisso
        }

        int digits = _padZero ? total.ToString().Length : 1;
        string number = index.ToString().PadLeft(digits, '0');

        return _position == Position.Suffix
            ? $"{name}{_separator}{number}"
            : $"{number}{_separator}{name}";
    }

    void OnGUI()
    {
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Numerazione oggetti", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        _position  = (Position)EditorGUILayout.EnumPopup("Posizione numero", _position);
        _separator = EditorGUILayout.TextField("Separatore", _separator);
        _startIndex = EditorGUILayout.IntField("Indice iniziale", _startIndex);
        _padZero   = EditorGUILayout.Toggle("Padding con zero (01, 02…)", _padZero);
        _stripExisting = EditorGUILayout.Toggle("Rimuovi numerazione esistente", _stripExisting);

        EditorGUILayout.Space(8);

        var names = SourceNames();
        int total = _startIndex + names.Count - 1;

        // Anteprima risultato
        EditorGUILayout.LabelField("Anteprima:", EditorStyles.boldLabel);
        _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Height(150));
        for (int i = 0; i < names.Count; i++)
        {
            string newName = BuildNumbered(names[i], _startIndex + i, total);
            EditorGUILayout.LabelField($"  {names[i]}  →  {newName}");
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(8);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Applica", GUILayout.Height(30))) { Apply(names, total); Close(); }
        if (GUILayout.Button("Annulla", GUILayout.Height(30))) Close();
        EditorGUILayout.EndHorizontal();
    }

    void Apply(List<string> names, int total)
    {
        if (_gos != null && _gos.Count > 0)
        {
            Undo.RecordObjects(_gos.Cast<Object>().ToArray(), "Numera GameObject");
            for (int i = 0; i < _gos.Count; i++)
                _gos[i].name = BuildNumbered(names[i], _startIndex + i, total);
        }
        else if (_assets != null && _assets.Count > 0)
        {
            AssetDatabase.StartAssetEditing();
            try
            {
                for (int i = 0; i < _assets.Count; i++)
                {
                    string newName = BuildNumbered(names[i], _startIndex + i, total);
                    string err = AssetDatabase.RenameAsset(_assets[i], newName);
                    if (!string.IsNullOrEmpty(err)) Debug.LogWarning($"[AssetManager] {_assets[i]}: {err}");
                }
            }
            finally { AssetDatabase.StopAssetEditing(); AssetDatabase.Refresh(); }
        }
    }
}