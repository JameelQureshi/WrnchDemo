/*
  Copyright (c) 2020 wrnch Inc.
  All rights reserved
 */

#ifndef WRNCH_LOGGING_C_API
#define WRNCH_LOGGING_C_API

#include <wrnch/definitions.h>


typedef enum
{
    wrLogLevel_INFO,
    wrLogLevel_WARNING,
    wrLogLevel_ERROR
} wrLogLevel;

#ifdef __cplusplus
extern "C"
{
#endif

    /// @brief Set the global log level used by wrnchAI.
    /// @param level the desired log level. If logLevel is not one of the enumerated values, this
    ///        function is a no-op.
    /// @note This function is thread-safe.
    WRNCH_DLL_INTERFACE void wrnchAI_SetLogLevel(wrLogLevel logLevel);

    /// @brief Get the global log level used by wrnchAI. Default value is wrLogLevel_WARNING
    /// @note This function is thread-safe.
    WRNCH_DLL_INTERFACE wrLogLevel wrnchAI_GetLogLevel(void);

#ifdef __cplusplus
}
#endif

#endif /* WRNCH_LOGGING_C_API */
