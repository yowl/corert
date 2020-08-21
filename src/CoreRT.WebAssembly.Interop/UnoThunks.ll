target datalayout = "e-m:e-p:32:32-i64:64-n32:64-S128"
target triple = "wasm32-unknown-emscripten"

declare i8 @Uno_Windows_UI_Core_CoreDispatcher__DispatcherCallback(i8*)

define void @CoreRT_WebAssembly_Interop_WebAssembly_Runtime__DispatcherLLVM(i8*) {

Block0:      
  call i8 @Uno_Windows_UI_Core_CoreDispatcher__DispatcherCallback(i8* %0)
  ret void
}

