// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "pal_types.h"
#include "pal_compiler.h"
#include "opensslshim.h"

extern "C" {
/*
Creates a new memory-backed BIO instance.
*/
PALEXPORT BIO* CryptoNative_CreateMemoryBio(void);

/*
Direct shim to BIO_new_file.
*/
PALEXPORT BIO* CryptoNative_BioNewFile(const char* filename, const char* mode);

/*
Cleans up and deletes a BIO instance.

Implemented by:
1) Calling BIO_free

No-op if a is null.
The given BIO pointer is invalid after this call.
*/
PALEXPORT int32_t CryptoNative_BioDestroy(BIO* a);

/*
Direct shim to BIO_gets.
*/
PALEXPORT int32_t CryptoNative_BioGets(BIO* b, char* buf, int32_t size);

/*
Direct shim to BIO_read.
*/
PALEXPORT int32_t CryptoNative_BioRead(BIO* b, void* buf, int32_t len);

/*
Direct shim to BIO_write.
*/
PALEXPORT int32_t CryptoNative_BioWrite(BIO* b, const void* buf, int32_t len);

/*
Gets the size of data available in the BIO.

Shims the BIO_get_mem_data method.
*/
PALEXPORT int32_t CryptoNative_GetMemoryBioSize(BIO* bio);

/*
Shims the BIO_ctrl_pending method.

Returns the number of pending characters in the BIOs read and write buffers.
*/
PALEXPORT int32_t CryptoNative_BioCtrlPending(BIO* bio);
}
