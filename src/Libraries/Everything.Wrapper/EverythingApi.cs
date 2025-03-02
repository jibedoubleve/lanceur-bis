using System.Runtime.InteropServices;
using System.Text;

namespace Everything.Wrapper;

public class EverythingApi : IEverythingApi
{
    #region Fields

    private const int EVERYTHING_ERROR_CREATETHREAD = 5;
    private const int EVERYTHING_ERROR_CREATEWINDOW = 4;
    private const int EVERYTHING_ERROR_INVALIDCALL = 7;
    private const int EVERYTHING_ERROR_INVALIDINDEX = 6;
    private const int EVERYTHING_ERROR_IPC = 2;
    private const int EVERYTHING_ERROR_MEMORY = 1;
    private const int EVERYTHING_ERROR_REGISTERCLASSEX = 3;
    private const int EVERYTHING_OK = 0;
    private const int EVERYTHING_REQUEST_ATTRIBUTES = 0x00000100;
    private const int EVERYTHING_REQUEST_DATE_ACCESSED = 0x00000080;
    private const int EVERYTHING_REQUEST_DATE_CREATED = 0x00000020;
    private const int EVERYTHING_REQUEST_DATE_MODIFIED = 0x00000040;
    private const int EVERYTHING_REQUEST_DATE_RECENTLY_CHANGED = 0x00001000;
    private const int EVERYTHING_REQUEST_DATE_RUN = 0x00000800;
    private const int EVERYTHING_REQUEST_EXTENSION = 0x00000008;
    private const int EVERYTHING_REQUEST_FILE_LIST_FILE_NAME = 0x00000200;
    private const int EVERYTHING_REQUEST_FILE_NAME = 0x00000001;
    private const int EVERYTHING_REQUEST_FULL_PATH_AND_FILE_NAME = 0x00000004;
    private const int EVERYTHING_REQUEST_HIGHLIGHTED_FILE_NAME = 0x00002000;
    private const int EVERYTHING_REQUEST_HIGHLIGHTED_FULL_PATH_AND_FILE_NAME = 0x00008000;
    private const int EVERYTHING_REQUEST_HIGHLIGHTED_PATH = 0x00004000;
    private const int EVERYTHING_REQUEST_PATH = 0x00000002;
    private const int EVERYTHING_REQUEST_RUN_COUNT = 0x00000400;
    private const int EVERYTHING_REQUEST_SIZE = 0x00000010;
    private const int EVERYTHING_SORT_ATTRIBUTES_ASCENDING = 15;
    private const int EVERYTHING_SORT_ATTRIBUTES_DESCENDING = 16;
    private const int EVERYTHING_SORT_DATE_ACCESSED_ASCENDING = 23;
    private const int EVERYTHING_SORT_DATE_ACCESSED_DESCENDING = 24;
    private const int EVERYTHING_SORT_DATE_CREATED_ASCENDING = 11;
    private const int EVERYTHING_SORT_DATE_CREATED_DESCENDING = 12;
    private const int EVERYTHING_SORT_DATE_MODIFIED_ASCENDING = 13;
    private const int EVERYTHING_SORT_DATE_MODIFIED_DESCENDING = 14;
    private const int EVERYTHING_SORT_DATE_RECENTLY_CHANGED_ASCENDING = 21;
    private const int EVERYTHING_SORT_DATE_RECENTLY_CHANGED_DESCENDING = 22;
    private const int EVERYTHING_SORT_DATE_RUN_ASCENDING = 25;
    private const int EVERYTHING_SORT_DATE_RUN_DESCENDING = 26;
    private const int EVERYTHING_SORT_EXTENSION_ASCENDING = 7;
    private const int EVERYTHING_SORT_EXTENSION_DESCENDING = 8;
    private const int EVERYTHING_SORT_FILE_LIST_FILENAME_ASCENDING = 17;
    private const int EVERYTHING_SORT_FILE_LIST_FILENAME_DESCENDING = 18;
    private const int EVERYTHING_SORT_NAME_ASCENDING = 1;
    private const int EVERYTHING_SORT_NAME_DESCENDING = 2;
    private const int EVERYTHING_SORT_PATH_ASCENDING = 3;
    private const int EVERYTHING_SORT_PATH_DESCENDING = 4;
    private const int EVERYTHING_SORT_RUN_COUNT_ASCENDING = 19;
    private const int EVERYTHING_SORT_RUN_COUNT_DESCENDING = 20;
    private const int EVERYTHING_SORT_SIZE_ASCENDING = 5;
    private const int EVERYTHING_SORT_SIZE_DESCENDING = 6;
    private const int EVERYTHING_SORT_TYPE_NAME_ASCENDING = 9;
    private const int EVERYTHING_SORT_TYPE_NAME_DESCENDING = 10;
    private const int EVERYTHING_TARGET_MACHINE_ARM = 3;
    private const int EVERYTHING_TARGET_MACHINE_X64 = 2;
    private const int EVERYTHING_TARGET_MACHINE_X86 = 1;

    #endregion

    #region Methods

    [DllImport("Everything64.dll")] private static extern uint Everything_GetLastError();

    [DllImport("Everything64.dll")] private static extern bool Everything_GetMatchCase();

    [DllImport("Everything64.dll")] private static extern bool Everything_GetMatchPath();

    [DllImport("Everything64.dll")] private static extern bool Everything_GetMatchWholeWord();

    [DllImport("Everything64.dll")] private static extern uint Everything_GetMax();

    [DllImport("Everything64.dll")] private static extern uint Everything_GetNumFileResults();

    [DllImport("Everything64.dll")] private static extern uint Everything_GetNumFolderResults();

    [DllImport("Everything64.dll")] private static extern uint Everything_GetNumResults();

    [DllImport("Everything64.dll")] private static extern uint Everything_GetOffset();

    [DllImport("Everything64.dll")] private static extern bool Everything_GetRegex();

    [DllImport("Everything64.dll")] private static extern uint Everything_GetRequestFlags();

    [DllImport("Everything64.dll")] private static extern uint Everything_GetResultAttributes(uint nIndex);

    [DllImport("Everything64.dll")] private static extern bool Everything_GetResultDateAccessed(uint nIndex, out long lpFileTime);

    [DllImport("Everything64.dll")] private static extern bool Everything_GetResultDateCreated(uint nIndex, out long lpFileTime);

    [DllImport("Everything64.dll")] private static extern bool Everything_GetResultDateModified(uint nIndex, out long lpFileTime);

    [DllImport("Everything64.dll")] private static extern bool Everything_GetResultDateRecentlyChanged(uint nIndex, out long lpFileTime);

    [DllImport("Everything64.dll")] private static extern bool Everything_GetResultDateRun(uint nIndex, out long lpFileTime);

    [DllImport("Everything64.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr Everything_GetResultExtension(uint nIndex);

    [DllImport("Everything64.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr Everything_GetResultFileListFileName(uint nIndex);

    [DllImport("Everything64.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr Everything_GetResultFileName(uint nIndex);

    [DllImport("Everything64.dll", CharSet = CharSet.Unicode)]
    private static extern void
        Everything_GetResultFullPathName(uint nIndex, StringBuilder lpString, uint nMaxCount);

    [DllImport("Everything64.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr Everything_GetResultHighlightedFileName(uint nIndex);

    [DllImport("Everything64.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr Everything_GetResultHighlightedFullPathAndFileName(uint nIndex);

    [DllImport("Everything64.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr Everything_GetResultHighlightedPath(uint nIndex);

    [DllImport("Everything64.dll")] private static extern uint Everything_GetResultListRequestFlags();

    [DllImport("Everything64.dll")] private static extern uint Everything_GetResultListSort();

    [DllImport("Everything64.dll")] private static extern uint Everything_GetResultRunCount(uint nIndex);

    [DllImport("Everything64.dll")] private static extern bool Everything_GetResultSize(uint nIndex, out long lpFileSize);

    [DllImport("Everything64.dll", CharSet = CharSet.Unicode)]
    private static extern uint Everything_GetRunCountFromFileName(string lpFileName);

    [DllImport("Everything64.dll")] private static extern IntPtr Everything_GetSearchW();

    [DllImport("Everything64.dll")] private static extern uint Everything_GetSort();

    [DllImport("Everything64.dll")] private static extern uint Everything_GetTotFileResults();

    [DllImport("Everything64.dll")] private static extern uint Everything_GetTotFolderResults();

    [DllImport("Everything64.dll")] private static extern uint Everything_GetTotResults();

    [DllImport("Everything64.dll", CharSet = CharSet.Unicode)]
    private static extern uint Everything_IncRunCountFromFileName(string lpFileName);

    [DllImport("Everything64.dll")] private static extern bool Everything_IsDBLoaded();

    [DllImport("Everything64.dll")] private static extern bool Everything_IsFileResult(uint nIndex);

    [DllImport("Everything64.dll")] private static extern bool Everything_IsFolderResult(uint nIndex);

    [DllImport("Everything64.dll")] private static extern bool Everything_IsVolumeResult(uint nIndex);

    [DllImport("Everything64.dll")] private static extern bool Everything_QueryW(bool bWait);

    [DllImport("Everything64.dll")] private static extern void Everything_Reset();

    [DllImport("Everything64.dll")] private static extern void Everything_SetMatchCase(bool bEnable);

    [DllImport("Everything64.dll")] private static extern void Everything_SetMatchPath(bool bEnable);

    [DllImport("Everything64.dll")] private static extern void Everything_SetMatchWholeWord(bool bEnable);

    [DllImport("Everything64.dll")] private static extern void Everything_SetMax(uint dwMax);

    [DllImport("Everything64.dll")] private static extern void Everything_SetOffset(uint dwOffset);

    [DllImport("Everything64.dll")] private static extern void Everything_SetRegex(bool bEnable);

    [DllImport("Everything64.dll")] private static extern void Everything_SetRequestFlags(uint dwRequestFlags);

    [DllImport("Everything64.dll", CharSet = CharSet.Unicode)]
    private static extern bool Everything_SetRunCountFromFileName(string lpFileName, uint dwRunCount);

    [DllImport("Everything64.dll", CharSet = CharSet.Unicode)]
    private static extern uint Everything_SetSearchW(string lpSearchString);

    // Everything 1.4
    [DllImport("Everything64.dll")] private static extern void Everything_SetSort(uint dwSortType);

    [DllImport("Everything64.dll")] private static extern void Everything_SortResultsByPath();

    private static ResultType GetResultType(uint resultIndex)
    {
        if (Everything_IsFileResult(resultIndex))
        {
            var extensionPtr = Everything_GetResultExtension(resultIndex);
            var extension = Marshal.PtrToStringUni(extensionPtr);

            return extension?.ToLower() switch
            {
                "exe"    => ResultType.Executable,
                "xls"    => ResultType.Excel,
                "xlsx"   => ResultType.Excel,
                "pdf"    => ResultType.Pdf,
                "zip"    => ResultType.Zip,
                "rar"    => ResultType.Zip,
                "gif"    => ResultType.Image,
                "jpg"    => ResultType.Image,
                "png"    => ResultType.Image,
                "doc"    => ResultType.Word,
                "docx"   => ResultType.Word,
                "cs"     => ResultType.Code,
                "csproj" => ResultType.Code,
                "js"     => ResultType.Code,
                "html"   => ResultType.Code,
                "xml"    => ResultType.Code,
                "json"   => ResultType.Code,
                "ps1"    => ResultType.Code,
                "txt"    => ResultType.Text,
                "mp3"    => ResultType.Music,
                _        => ResultType.File
            };
        }

        if (Everything_IsFolderResult(resultIndex)) return ResultType.Directory;


        return ResultType.File;
    }

    public ResultSet Search(string query)
    {
        if (!Everything_IsDBLoaded())
        {
            return ResultSet.Failure("Unable to connect to the Everything database.");
        }
        
        uint i;
        const int fileAndPathSize = 260;

        Everything_SetMax(5_000);
        _ = Everything_SetSearchW(query);
        Everything_SetRequestFlags(
            EVERYTHING_REQUEST_FILE_NAME |
            EVERYTHING_REQUEST_PATH |
            EVERYTHING_REQUEST_EXTENSION
        );
        Everything_SetSort(13);
        Everything_QueryW(true);

        var resultCount = Everything_GetNumResults();
        var result = new ResultSet();

        for (i = 0; i < resultCount; i++)
        {
            var stringBuilder = new StringBuilder(fileAndPathSize);
            Everything_GetResultFullPathName(i, stringBuilder, fileAndPathSize);

            result.Add(new() { Name = Marshal.PtrToStringUni(Everything_GetResultFileName(i)) ?? string.Empty, Path = stringBuilder.ToString(), ResultType = GetResultType(i) });
        }
        
        return result;
    }

    #endregion
}