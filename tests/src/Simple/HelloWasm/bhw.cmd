pusdh .
cd E:\GitHub\corert\tests\src\Simple\HelloWasm
E:\GitHub\corert\Tools\dotnetcli\dotnet.exe exec "E:\GitHub\corert\packages\microsoft.net.compilers.toolset\3.3.0-beta2-19367-02\build\..\tasks\netcoreapp2.1\bincore\csc.dll" /noconfig /unsafe+ /nowarn:1701,1702 /nostdlib+ /warn:4 /define:WASM;BIT32;PLATFORM_UNIX;CORERT;DEBUG;TRACE;PLATFORM_WINDOWS;DEBUGRESOURCES;SIGNED /reference:E:\GitHub\corert\tests\src\Simple\HelloWasm\bin\Debug\wasm\CpObj.dll /reference:E:\GitHub\corert\tests\src\Simple\HelloWasm\bin\Debug\wasm\ILHelpers.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\Microsoft.CSharp.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\Microsoft.VisualBasic.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\Microsoft.Win32.Primitives.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\mscorlib.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\netstandard.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.AppContext.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Buffers.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Collections.Concurrent.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Collections.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Collections.Immutable.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Collections.NonGeneric.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Collections.Specialized.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.ComponentModel.Annotations.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.ComponentModel.DataAnnotations.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.ComponentModel.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.ComponentModel.EventBasedAsync.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.ComponentModel.Primitives.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.ComponentModel.TypeConverter.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Configuration.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Console.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Core.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Data.Common.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Data.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Diagnostics.Contracts.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Diagnostics.Debug.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Diagnostics.DiagnosticSource.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Diagnostics.FileVersionInfo.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Diagnostics.Process.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Diagnostics.StackTrace.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Diagnostics.TextWriterTraceListener.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Diagnostics.Tools.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Diagnostics.TraceSource.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Diagnostics.Tracing.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Drawing.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Drawing.Primitives.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Dynamic.Runtime.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Globalization.Calendars.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Globalization.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Globalization.Extensions.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.IO.Compression.Brotli.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.IO.Compression.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.IO.Compression.FileSystem.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.IO.Compression.ZipFile.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.IO.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.IO.FileSystem.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.IO.FileSystem.DriveInfo.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.IO.FileSystem.Primitives.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.IO.FileSystem.Watcher.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.IO.IsolatedStorage.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.IO.MemoryMappedFiles.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.IO.Pipes.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.IO.UnmanagedMemoryStream.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Linq.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Linq.Expressions.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Linq.Parallel.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Linq.Queryable.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Memory.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Net.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Net.Http.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Net.HttpListener.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Net.Mail.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Net.NameResolution.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Net.NetworkInformation.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Net.Ping.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Net.Primitives.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Net.Requests.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Net.Security.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Net.ServicePoint.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Net.Sockets.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Net.WebClient.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Net.WebHeaderCollection.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Net.WebProxy.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Net.WebSockets.Client.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Net.WebSockets.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Numerics.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Numerics.Vectors.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.ObjectModel.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Reflection.DispatchProxy.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Reflection.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Reflection.Emit.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Reflection.Emit.ILGeneration.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Reflection.Emit.Lightweight.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Reflection.Extensions.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Reflection.Metadata.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Reflection.Primitives.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Reflection.TypeExtensions.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Resources.Reader.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Resources.ResourceManager.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Resources.Writer.dll /reference:E:\GitHub\corert\packages\System.Runtime.CompilerServices.Unsafe\4.5.1\ref\netstandard2.0\System.Runtime.CompilerServices.Unsafe.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Runtime.CompilerServices.VisualC.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Runtime.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Runtime.Extensions.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Runtime.Handles.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Runtime.InteropServices.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Runtime.InteropServices.RuntimeInformation.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Runtime.InteropServices.WindowsRuntime.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Runtime.Loader.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Runtime.Numerics.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Runtime.Serialization.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Runtime.Serialization.Formatters.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Runtime.Serialization.Json.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Runtime.Serialization.Primitives.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Runtime.Serialization.Xml.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Security.Claims.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Security.Cryptography.Algorithms.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Security.Cryptography.Csp.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Security.Cryptography.Encoding.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Security.Cryptography.Primitives.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Security.Cryptography.X509Certificates.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Security.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Security.Principal.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Security.SecureString.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.ServiceModel.Web.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.ServiceProcess.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Text.Encoding.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Text.Encoding.Extensions.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Text.RegularExpressions.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Threading.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Threading.Overlapped.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Threading.Tasks.Dataflow.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Threading.Tasks.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Threading.Tasks.Extensions.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Threading.Tasks.Parallel.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Threading.Thread.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Threading.ThreadPool.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Threading.Timer.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Transactions.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Transactions.Local.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.ValueTuple.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Web.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Web.HttpUtility.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Windows.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Xml.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Xml.Linq.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Xml.ReaderWriter.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Xml.Serialization.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Xml.XDocument.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Xml.XmlDocument.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Xml.XmlSerializer.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Xml.XPath.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\System.Xml.XPath.XDocument.dll /reference:E:\GitHub\corert\packages\Microsoft.NETCore.App\2.1.11\ref\netcoreapp2.1\WindowsBase.dll /debug+ /debug:portable /optimize- /out:E:\GitHub\corert\tests\src\Simple\HelloWasm\obj\Debug\wasm\HelloWasm.exe /target:exe /warnaserror+ /utf8output /checksumalgorithm:SHA256 /langversion:preview /analyzerconfig:E:\GitHub\corert\.editorconfig Program.cs /reference:E:\GitHub\corert\src\CoreRT.WebAssembly.Interop\bin\Debug\netstandard2.0\CoreRT.WebAssembly.Interop.dll /reference:E:\GitHub\corert\packages\microsoft.jsinterop\3.1.0\lib\netstandard2.0\Microsoft.JsInterop.dll





popd