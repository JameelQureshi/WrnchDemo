/*
 Copyright (c) 2020 wrnch Inc.
 All rights reserved
*/

#ifndef WRNCH_LOGGING_CXX_API
#define WRNCH_LOGGING_CXX_API

#include <wrnch/logging.h>


namespace wrnch
{
/// @brief Set the global log level used by wrnchAI.
/// @param level the desired log level. If logLevel is not one of the enumerated values, this
///        function is a no-op.
/// @note This function is thread-safe.
inline void SetLogLevel(wrLogLevel level) { wrnchAI_SetLogLevel(level); }

/// @brief Get the global log level used by wrnchAI. Default value is wrLogLevel_WARNING
/// @note This function is thread-safe.
inline wrLogLevel GetLogLevel() { return wrnchAI_GetLogLevel(); }

}  // namespace wrnch

#endif /* WRNCH_LOGGING_CXX_API  */
