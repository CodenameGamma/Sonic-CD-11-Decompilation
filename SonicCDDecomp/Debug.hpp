#ifndef DEBUG_H
#define DEBUG_H

inline void printLog(const char *msg, ...)
{
    if (engineDebugMode) {
        char buffer[0x100];

        // make the full string
        va_list args;
        va_start(args, msg);
        vsprintf(buffer, msg, args);
        printf("%s\n", buffer);
        sprintf(buffer, "%s\n", buffer);

        char pathBuffer[0x100];
#if RETRO_PLATFORM == RETRO_OSX
        if (!usingCWD)
            sprintf(pathBuffer, "%s/log.txt", getResourcesPath());
        else
            sprintf(pathBuffer, "log.txt");
#else
        sprintf(pathBuffer, "log.txt");
#endif
        FileIO *file = fOpen(pathBuffer, "a");
        if (file) {
            fWrite(&buffer, 1, StrLength(buffer), file);
            fClose(file);
        }
    }
}

enum DevMenuMenus {
    DEVMENU_MAIN,
    DEVMENU_PLAYERSEL,
    DEVMENU_STAGELISTSEL,
    DEVMENU_STAGESEL,
    DEVMENU_SCRIPTERROR,
};

void initDevMenu();
void initErrorMessage();
void processStageSelect();

#endif //!DEBUG_H
