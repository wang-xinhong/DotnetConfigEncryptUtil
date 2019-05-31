using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CN.Wangxinhong.DotnetConfigEncUtil
{
    /// <summary>
    /// ReturnCode Enum
    /// </summary>
    enum ReturnResult : int
    {
        /// <summary>
        /// success return
        /// </summary>
        SUCCESS = 0,

        /// <summary>
        /// no action was taken
        /// </summary>
        NOACTION_TAKEN = 1,

        /// <summary>
        /// params is null
        /// </summary>
        PARAMS_NULL = -1,

        /// <summary>
        /// params is out of range
        /// </summary>
        PARAMS_OUT_OF_RANGE = -2,

        /// <summary>
        /// config section is not found
        /// </summary>
        CONFIG_SECTION_NOT_FOUND = -3,

        /// <summary>
        /// internal error with exception
        /// </summary>
        EXCEPTION_DETECTED = -4,

    }
}
