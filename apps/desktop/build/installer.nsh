!include LogicLib.nsh
!include nsDialogs.nsh
!include WinMessages.nsh

!ifndef BUILD_UNINSTALLER
Var DesktopShortcutCheckbox
Var DesktopShortcutState
Var StartMenuShortcutCheckbox
Var StartMenuShortcutState
Var StartAtLoginCheckbox
Var StartAtLoginState

Function MiLuStartupOptionsPage
  nsDialogs::Create 1018
  Pop $0
  ${If} $0 == error
    Abort
  ${EndIf}

  ${NSD_CreateLabel} 0 0 100% 24u "Choose optional Windows integration."
  Pop $1
  ${NSD_CreateCheckbox} 0 34u 100% 12u "Create a desktop shortcut"
  Pop $DesktopShortcutCheckbox
  ${NSD_Check} $DesktopShortcutCheckbox
  ${NSD_CreateCheckbox} 0 52u 100% 12u "Create a Start Menu shortcut"
  Pop $StartMenuShortcutCheckbox
  ${NSD_Check} $StartMenuShortcutCheckbox
  ${NSD_CreateCheckbox} 0 70u 100% 12u "Start MiLuStudio when Windows starts"
  Pop $StartAtLoginCheckbox
  nsDialogs::Show
FunctionEnd

Function MiLuStartupOptionsPageLeave
  ${NSD_GetState} $DesktopShortcutCheckbox $DesktopShortcutState
  ${NSD_GetState} $StartMenuShortcutCheckbox $StartMenuShortcutState
  ${NSD_GetState} $StartAtLoginCheckbox $StartAtLoginState
FunctionEnd

!macro customWelcomePage
  !insertmacro MUI_PAGE_WELCOME
  Page custom MiLuStartupOptionsPage MiLuStartupOptionsPageLeave
!macroend
!endif

!macro customInstall
  ${If} $DesktopShortcutState != ${BST_CHECKED}
    Delete "$newDesktopLink"
  ${EndIf}
  ${If} $StartMenuShortcutState != ${BST_CHECKED}
    Delete "$newStartMenuLink"
  ${EndIf}
  ${If} $StartAtLoginState == ${BST_CHECKED}
    CreateShortCut "$SMSTARTUP\MiLuStudio.lnk" "$INSTDIR\MiLuStudio.exe"
    WinShell::SetLnkAUMI "$SMSTARTUP\MiLuStudio.lnk" "${APP_ID}"
  ${Else}
    Delete "$SMSTARTUP\MiLuStudio.lnk"
  ${EndIf}
!macroend

!macro customUnInstall
  Delete "$SMSTARTUP\MiLuStudio.lnk"
!macroend
