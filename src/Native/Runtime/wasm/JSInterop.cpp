/* trying without this and going from extern straight to js as done for Microsoft.JSInterop 
#include <emscripten.h>

 *
* a copy of mono_wasm_invoke_js from driver.c *
static char*
corert_wasm_invoke_js (char *str, int *is_exception)
{
	if (str == NULL)
		return NULL;

	char *native_val = mono_string_to_utf8 (str);
	mono_unichar2 *native_res = (mono_unichar2*)EM_ASM_INT ({
		var str = UTF8ToString ($0);
		try {
			var res = eval (str);
			if (res === null || res == undefined)
				return 0;
			res = res.toString ();
			setValue ($1, 0, "i32");
		} catch (e) {
			res = e.toString ();
			setValue ($1, 1, "i32");
			if (res === null || res === undefined)
				res = "unknown exception";
		}
		var buff = Module._malloc((res.length + 1) * 2);
		stringToUTF16 (res, buff, (res.length + 1) * 2);
		return buff;
	}, (int)native_val, is_exception);

	mono_free (native_val);

	if (native_res == NULL)
		return NULL;

	MonoString *res = mono_string_from_utf16 (native_res);
	free (native_res);
	return res;
    
}
*/
