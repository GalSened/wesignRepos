export enum FLOW_STEP {
    NONE = 0,
    SELF_SIGN_UPLOAD = 1,
    SELF_SIGN_PLACE_FIELDS = 2,
    SELF_SIGN_SIGN = 3,
    SELF_SIGN_DOWNLOAD = 4,
    TEMPLATE_UPLOAD = 5,
    TEMPLATE_EDIT = 6,
    ONLINE_SELECT = 7,
    ONLINE_ASSIGN = 8,
    ONLINE_SEND_GUIDE = 9,
    MULTISIGN_SELECT = 10,
    MULTISIGN_ASSIGN = 11,
    MULTISIGN_SEND = 12,
    TINY_SIGN_UPLOAD = 13,
    TINY_SIGN_PLACE_FIELDS = 14
}

export type FLOW_NAMES =
    "" | "none" | "selfsign" | "onlinesign" | "workflowsign" | "template" | "tinysign";
