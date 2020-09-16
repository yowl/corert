// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#include "common.h"
#include "gcenv.h"

// Emscripten does not provide a complete implementation of mmap and munmap: munmap cannot unmap partial allocations
// Emscripten does provide an implementation of posix_memalign which is used here.

// Reserve virtual memory range.
// Parameters:
//  size      - size of the virtual memory range
//  alignment - requested memory alignment, 0 means no specific alignment requested
//  flags     - flags to control special settings like write watching
// Return:
//  Starting virtual address of the reserved range
static void* VirtualReserveInner(size_t size, size_t alignment, uint32_t flags)
{
    assert(!(flags & VirtualReserveFlags::WriteWatch) && "WriteWatch not supported on Wasm");
    if (alignment == 0)
    {
        alignment = OS_PAGE_SIZE;
    }

    void * pRetVal;
    int result = posix_memalign(&pRetVal, alignment, size);
    if(result != 0)
    {
        return NULL; // failed
    }
    memset(pRetVal, 0, size);
    return pRetVal;
}

// Reserve virtual memory range.
// Parameters:
//  size      - size of the virtual memory range
//  alignment - requested memory alignment, 0 means no specific alignment requested
//  flags     - flags to control special settings like write watching
//  node      - the NUMA node to reserve memory on
// Return:
//  Starting virtual address of the reserved range
void* GCToOSInterface::VirtualReserve(size_t size, size_t alignment, uint32_t flags, uint16_t node)
{
    return VirtualReserveInner(size, alignment, flags);
}

// Release virtual memory range previously reserved using VirtualReserve
// Parameters:
//  address - starting virtual address
//  size    - size of the virtual memory range - ignored: emscripten does not support partial unmapping
// Return:
//  true if it has succeeded, false if it has failed
bool GCToOSInterface::VirtualRelease(void* address, size_t size)
{
    free(address);

    return TRUE; // free() is void
}

// Commit virtual memory range.
// Parameters:
//  size      - size of the virtual memory range
// Return:
//  Starting virtual address of the committed range
void* GCToOSInterface::VirtualReserveAndCommitLargePages(size_t size)
{
    // Wasm has no concept of large pages
    void* pRetVal = VirtualReserveInner(size, OS_PAGE_SIZE, 0);
    if (VirtualCommit(pRetVal, size, NUMA_NODE_UNDEFINED))
    {
        return pRetVal;
    }

    return nullptr;
}

// Commit virtual memory range. For emscripten this is not implemented
// Parameters:
//  address - starting virtual address
//  size    - size of the virtual memory range
// Return:
//  true if it has succeeded, false if it has failed
bool GCToOSInterface::VirtualCommit(void* address, size_t size, uint16_t node)
{
    return TRUE;
}

// Decomit virtual memory range.  For emscripten this is not implemented
// Parameters:
//  address - starting virtual address
//  size    - size of the virtual memory range
// Return:
//  true if it has succeeded, false if it has failed
bool GCToOSInterface::VirtualDecommit(void* address, size_t size)
{
    return TRUE;
}
