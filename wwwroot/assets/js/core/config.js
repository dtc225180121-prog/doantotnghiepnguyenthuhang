const DEPLOYED_API_ORIGIN = "https://duancuahang-vvmv.onrender.com";

const isLocalFrontend =
    window.location.protocol === "file:" ||
    (["localhost", "127.0.0.1"].includes(window.location.hostname) &&
        window.location.port !== "5041");

const isBackendHost =
    ["localhost", "127.0.0.1"].includes(window.location.hostname) &&
    window.location.port === "5041";

const API_ORIGIN = isLocalFrontend
    ? "http://localhost:5041"
    : isBackendHost
        ? window.location.origin
        : DEPLOYED_API_ORIGIN;

const CONFIG = {
    API_BASE: API_ORIGIN + "/api",

    ROLES: {
        TEACHER: "teacher",
        STUDENT: "student"
    }
};

window.CONFIG = CONFIG;
