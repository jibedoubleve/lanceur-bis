using ICSharpCode.AvalonEdit.CodeCompletion;

namespace Lanceur.Ui.WPF.SyntaxColouration;

public static class CompletionDataListExtensions
{
    #region Fields

    private static readonly List<LuaCompletionData> ContextCompletion;

    private static readonly List<LuaCompletionData> FunctionsCompletion;

    #endregion

    #region Constructors

    static CompletionDataListExtensions()
    {
        ContextCompletion = new()
        {
            new("FileName", "Path to the file to execute or the URL"),
            new("Parameters", "The parameters of the command to execute")
        };

        FunctionsCompletion = new()
        {
            new("abs"),
            new("acos"),
            new("acos"),
            new("asin"),
            new("asin"),
            new("assert"),
            new("atan"),
            new("atan"),
            new("atan2"),
            new("atan2"),
            new("ceil"),
            new("collectgarbage"),
            new("cos"),
            new("cos"),
            new("date"),
            new("debugbreak"),
            new("debugdump"),
            new("debughook"),
            new("debuginfo"),
            new("debugload"),
            new("debuglocals"),
            new("debugprint"),
            new("debugprofilestart"),
            new("debugprofilestop"),
            new("debugstack"),
            new("debugtimestamp"),
            new("deg"),
            new("difftime"),
            new("error"),
            new("exp"),
            new("floor"),
            new("forceinsecure"),
            new("foreach"),
            new("foreachi"),
            new("format"),
            new("frexp"),
            new("gcinfo"),
            new("geterrorhandler"),
            new("getfenv"),
            new("getglobal"),
            new("getmetatable"),
            new("getn"),
            new("getprinthandler"),
            new("gmatch"),
            new("gsub"),
            new("hooksecurefunc"),
            new("ipairs"),
            new("issecure"),
            new("issecurevariable"),
            new("ldexp"),
            new("loadstring"),
            new("log"),
            new("log10"),
            new("max"),
            new("message"),
            new("min"),
            new("mod"),
            new("newproxy"),
            new("next"),
            new("pairs"),
            new("pcall"),
            new("print"),
            new("rad"),
            new("random"),
            new("rawequal"),
            new("rawget"),
            new("rawset"),
            new("scrub"),
            new("securecall"),
            new("select"),
            new("seterrorhandler"),
            new("setfenv"),
            new("setglobal"),
            new("setmetatable"),
            new("setprinthandler"),
            new("sin"),
            new("sin"),
            new("sort"),
            new("sqrt"),
            new("strbyte"),
            new("strchar"),
            new("strconcat"),
            new("strfind"),
            new("strjoin"),
            new("strlen"),
            new("strlenutf8"),
            new("strlower"),
            new("strmatch"),
            new("strrep"),
            new("strrev"),
            new("strsplit"),
            new("strsub"),
            new("strtrim"),
            new("strupper"),
            new("tContains"),
            new("tDelete"),
            new("tan"),
            new("tan"),
            new("time"),
            new("tinsert"),
            new("tonumber"),
            new("tostring"),
            new("tostringall"),
            new("tremove"),
            new("type"),
            new("unpack"),
            new("wipe"),
            new("xpcall"),
            new("table"),
            new("insert"),
            new("remove"),
            new("setn"),
            new("getn"),
            new("foreach"),
            new("foreachi"),
            new("bit"),
            new("bit"),
            new("band"),
            new("bor"),
            new("bnot"),
            new("bxor")
        };
    }

    #endregion

    #region Methods

    public static void FillContextFields(this IList<ICompletionData> data)
    {
        foreach (var item in ContextCompletion) data.Add(item);
    }

    public static void FillFunctions(this IList<ICompletionData> data)
    {
        foreach (var item in FunctionsCompletion) data.Add(item);
    }

    public static bool IsContextKeyword(this string codeBuffer) => codeBuffer.StartsWith("context");

    #endregion
}