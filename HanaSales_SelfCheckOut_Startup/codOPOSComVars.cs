public struct OPOS_RESPONSE
{
    public const long OPOS_SUCCESS = 0;
    public const long OPOS_E_CLOSED = 101;
    public const long OPOS_E_CLAIMED = 102;
    public const long OPOS_E_NOTCLAIMED = 103;
    public const long OPOS_E_NOSERVICE = 104;
    public const long OPOS_E_DISABLED = 105;
    public const long OPOS_E_ILLEGAL = 106;
    public const long OPOS_E_NOHARDWARE = 107;
    public const long OPOS_E_OFFLINE = 108;
    public const long OPOS_E_NOEXIST = 109;
    public const long OPOS_E_EXISTS = 110;
    public const long OPOS_E_FAILURE = 111;
    public const long OPOS_E_TIMEOUT = 112;
    public const long OPOS_E_BUSY = 113;
    public const long OPOS_E_EXTENDED = 114;
    public const long OPOSERR = 100;           // Base for ResultCode errors.
    public const long OPOSERREXT = 200;        // Base for ResultCodeExtendedErrors.
}

public enum OPOS_CODE_DEFINE
{
    OPOS_SUCCESS = 0,
    OPOS_E_CLOSED = 101,
    OPOS_E_CLAIMED = 102,
    OPOS_E_NOTCLAIMED = 103,
    OPOS_E_NOSERVICE = 104,
    OPOS_E_DISABLED = 105,
    OPOS_E_ILLEGAL = 106,
    OPOS_E_NOHARDWARE = 107,
    OPOS_E_OFFLINE = 108,
    OPOS_E_NOEXIST = 109,
    OPOS_E_EXISTS = 110,
    OPOS_E_FAILURE = 111,
    OPOS_E_TIMEOUT = 112,
    OPOS_E_BUSY = 113,
    OPOS_E_EXTENDED = 114,
    OPOSERR = 100,           // Base for ResultCode errors.
    OPOSERREXT = 200        // Base for ResultCodeExtendedErrors.
}

public enum OPOS_SCALE_VALUES
{
    SCAL_SN_DISABLED = 1,
    SCAL_SN_ENABLED = 2
}

public enum OPOS_SCALE_UNITS
{
    SCAL_WU_GRAM = 1,
    SCAL_WU_KILOGRAM = 2,
    SCAL_WU_OUNCE = 3,
    SCAL_WU_POUND = 4
}

public enum OPOS_SCALE_STATUS
{
    SCAL_SUE_STABLE_WEIGHT = 11,
    SCAL_SUE_WEIGHT_UNSTABLE = 12,
    SCAL_SUE_WEIGHT_ZERO = 13,
    SCAL_SUE_WEIGHT_OVERWEIGHT = 14,
    SCAL_SUE_NOT_READY = 15,
    SCAL_SUE_WEIGHT_UNDER_ZERO = 16
}
