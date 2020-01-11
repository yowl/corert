static const int64_t TICKS_PER_SECOND = 10000000; /* 10^7 */
#if HAVE_CLOCK_REALTIME
static const int64_t NANOSECONDS_PER_TICK = 100;
#else
static const int64_t TICKS_PER_MICROSECOND = 10; /* 1000 / 100 */
#endif

int64_t SystemNative_GetSystemTimeAsTicks()
{
#if HAVE_CLOCK_REALTIME
    struct timespec time;
    if (clock_gettime(CLOCK_REALTIME, &time) == 0)
    {
        return (int64_t)(time.tv_sec) * TICKS_PER_SECOND + (time.tv_nsec / NANOSECONDS_PER_TICK);
    }
#else
    struct timeval time;
    if (gettimeofday(&time, NULL) == 0)
    {
        return (int64_t)(time.tv_sec) * TICKS_PER_SECOND + (time.tv_usec * TICKS_PER_MICROSECOND);
    }
#endif
    // in failure we return 00:00 01 January 1970 UTC (Unix epoch)
    return 0;
}
