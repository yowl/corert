int SystemNative_CloseNetworkChangeListenerSocket (int a) { return 0; }
int SystemNative_CreateNetworkChangeListenerSocket (int a) { return 0; }
void SystemNative_ReadEvents (int a,int b) {}
int SystemNative_SchedGetAffinity (int a,int b) { return 0; }
int SystemNative_SchedSetAffinity (int a,int b) { return 0; }

/* kerberos stuff.  Runtime linking fails if these are not found */
int NetSecurityNative_AcceptSecContext(void* a, void* b, void* c, void* d, void* e, void* f, void* g, void* h) { return 0;}
int NetSecurityNative_AcquireAcceptorCred(void* a, void* b) { return 0; }
int NetSecurityNative_DeleteSecContext(void* a, void* b) { return 0; }
int NetSecurityNative_DisplayMajorStatus(void* a, int i, void* b) { return 0; }
int NetSecurityNative_DisplayMinorStatus(void* a, int i, void* b) { return 0; }
int NetSecurityNative_ImportPrincipalName(void* a, void* b, void* c, void* d) { return 0; }
int NetSecurityNative_ImportUserName(void* a, void* b, int c, void* d) { return 0; }
int NetSecurityNative_InitSecContext(void* a, void* b, void* c, int d, void* e, int f, void* g, void* h, void *i, void *j, void *k) { return 0; }
int NetSecurityNative_InitSecContextEx(void* a, void* b, void* c, int d, void* e, int f, void* g, int h, void* i, void* j, void* k, void *l, void *m) { return 0; }
int NetSecurityNative_InitiateCredSpNego(void* a, void* b, void* c) { return 0; }
int NetSecurityNative_InitiateCredWithPassword(void* a, int b, void* c, void* d, int e, void* f) { return 0; }
int NetSecurityNative_IsNtlmInstalled() { return 0; }
int NetSecurityNative_ReleaseCred(void* a, void* b) { return 0; }
int NetSecurityNative_ReleaseGssBuffer(void* a, long b) { return 0; }
int NetSecurityNative_ReleaseName(void* a, void* b) { return 0; }

// MSQuic
int MsQuicOpen(int version, void* registration) { return 0; }
