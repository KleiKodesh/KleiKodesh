!include MUI2.nsh
!include nsDialogs.nsh
!include LogicLib.nsh

Name "Otzaria Installer"
OutFile "OtzariaSetup.exe"
InstallDir "$PROGRAMFILES\Otzaria"

!define APP_NAME "Otzaria"
!define REG_ROOT "HKCU"
!define REG_PATH "Software\${APP_NAME}\RibbonSettings"

!define MUI_ICON "Klei_Kodesh.ico"
!define MUI_UNICON "Klei_Kodesh.ico"
!define MUI_HEADERIMAGE

Page custom SettingsPage CreateSettingsPage
Page instfiles

Var Ck1
Var Ck2
Var Ck3
Var Rb1
Var Rb2
Var Rb3

Var Visible1  ; אוצרניק
Var Visible2  ; דרך האתרים
Var Visible3  ; היברו בוקס
Var DefaultOption  ; ערך ברירת מחדל (0,1,2)

Function CreateSettingsPage
  nsDialogs::Create 1018
  Pop $0
  ${If} $0 == error
    Abort
  ${EndIf}

  nsDialogs::SetRTL 1

  ${NSD_CreateLabel} 10u 10u 200u 12u "רכיבים פעילים:"
  Pop $0

  ${NSD_CreateCheckbox} 10u 25u 200u 12u "אוצרניק"
  Pop $Ck1

  ${NSD_CreateCheckbox} 10u 40u 200u 12u "דרך האתרים"
  Pop $Ck2

  ${NSD_CreateCheckbox} 10u 55u 200u 12u "היברו בוקס"
  Pop $Ck3

  ${NSD_CreateLabel} 10u 75u 200u 12u "ברירת מחדל:"

  ${NSD_CreateRadioButton} 10u 90u 200u 12u "אוצרניק"
  Pop $Rb1

  ${NSD_CreateRadioButton} 10u 105u 200u 12u "דרך האתרים"
  Pop $Rb2

  ${NSD_CreateRadioButton} 10u 120u 200u 12u "היברו בוקס"
  Pop $Rb3

  nsDialogs::Show
FunctionEnd

Function SettingsPage
  ${NSD_GetState} $Ck1 $Visible1
  ${NSD_GetState} $Ck2 $Visible2
  ${NSD_GetState} $Ck3 $Visible3

  ${NSD_GetState} $Rb1 $0
  ${If} $0 == ${BST_CHECKED}
    StrCpy $DefaultOption 0
  ${EndIf}

  ${NSD_GetState} $Rb2 $0
  ${If} $0 == ${BST_CHECKED}
    StrCpy $DefaultOption 1
  ${EndIf}

  ${NSD_GetState} $Rb3 $0
  ${If} $0 == ${BST_CHECKED}
    StrCpy $DefaultOption 2
  ${EndIf}
FunctionEnd

Section "Install"
  ; ShowOtzarnik
  ${If} $Visible1 == ${BST_CHECKED}
    ${If} $DefaultOption == 0
      WriteRegStr ${REG_ROOT} "${REG_PATH}" "ShowOtzarnik" "1,1"
    ${Else}
      WriteRegStr ${REG_ROOT} "${REG_PATH}" "ShowOtzarnik" "1,0"
    ${EndIf}
  ${Else}
    WriteRegStr ${REG_ROOT} "${REG_PATH}" "ShowOtzarnik" "0,0"
  ${EndIf}

  ; ShowWebSites
  ${If} $Visible2 == ${BST_CHECKED}
    ${If} $DefaultOption == 1
      WriteRegStr ${REG_ROOT} "${REG_PATH}" "ShowWebSites" "1,1"
    ${Else}
      WriteRegStr ${REG_ROOT} "${REG_PATH}" "ShowWebSites" "1,0"
    ${EndIf}
  ${Else}
    WriteRegStr ${REG_ROOT} "${REG_PATH}" "ShowWebSites" "0,0"
  ${EndIf}

  ; ShowHebrewBooks
  ${If} $Visible3 == ${BST_CHECKED}
    ${If} $DefaultOption == 2
      WriteRegStr ${REG_ROOT} "${REG_PATH}" "ShowHebrewBooks" "1,1"
    ${Else}
      WriteRegStr ${REG_ROOT} "${REG_PATH}" "ShowHebrewBooks" "1,0"
    ${EndIf}
  ${Else}
    WriteRegStr ${REG_ROOT} "${REG_PATH}" "ShowHebrewBooks" "0,0"
  ${EndIf}

  DetailPrint "התקנה בוצעה בהצלחה עם ההגדרות שנבחרו."
SectionEnd

Section "Uninstall"
  DeleteRegKey ${REG_ROOT} "${REG_PATH}"
SectionEnd
