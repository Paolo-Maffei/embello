\ tft driver for R61509U chip, connected as 16-bit parallel via FSMC

$A0000018 constant FSMC-BCR4
$A000001C constant FSMC-BTR4
$A000001C constant FSMC-BWTR4

$6C0007FE constant LCD-REG
$6C000800 constant LCD-RAM

: tft-fsmc ( -- )  \ configure the FSMC, SRAM bank 4
  $80               \ keep reset value
\                   \ FSMC_DataAddressMux_Disable
\                   \ FSMC_MemoryType_SRAM
  %01 4 lshift or   \ FSMC_MemoryDataWidth_16b
\                   \ FSMC_BurstAccessMode_Disable
\                   \ FSMC_WaitSignalPolarity_Low
\                   \ FSMC_WrapMode_Disable
\                   \ FSMC_WaitSignalActive_BeforeWaitState
  1 12 lshift or    \ FSMC_WriteOperation_Enable
\                   \ FSMC_WaitSignal_Disable
  1 14 lshift or    \ FSMC_ExtendedMode_Enable
\                   \ FSMC_AsynchronousWait_Disable
\                   \ FSMC_WriteBurst_Disable
  FSMC-BCR4 !

  0
  1 0 lshift or     \ FSMC_AddressSetupTime = 2x HCLK
\                   \ FSMC_AddressHoldTime = 0
  15 8 lshift or    \ FSMC_DataSetupTime = 16x HCLK
\                   \ FSMC_BusTurnAroundDuration = 0x00
\                   \ FSMC_CLKDivision = 0x00
\                   \ FSMC_DataLatency = 0x00
\                   \ FSMC_AccessMode_A
  FSMC-BTR4 !

  0
  0 0 lshift or     \ FSMC_AddressSetupTime = 1x HCLK
\                   \ FSMC_AddressHoldTime = 0
  3 8 lshift or     \ FSMC_DataSetupTime = 4x HCLK
\                   \ FSMC_BusTurnAroundDuration = 0x00
\                   \ FSMC_CLKDivision = 0x00
\                   \ FSMC_DataLatency = 0x00
\                   \ FSMC_AccessMode_A
  FSMC-BWTR4 !

  1 FSMC-BCR4 bis!  \ MBKEN:Memorybankenablebit
;

create tft:R61509U
hex
    400 h, 6200 h, 008 h, 0808 h, 300 h, 0005 h, 301 h, 4C06 h, 302 h, 0602 h,
    303 h, 050C h, 304 h, 3300 h, 305 h, 0C05 h, 306 h, 4206 h, 307 h, 060C h,
    308 h, 0500 h, 309 h, 0033 h, 010 h, 0014 h, 011 h, 0101 h, 012 h, 0000 h,
    013 h, 0001 h, 100 h, 0330 h, 101 h, 0247 h, 103 h, 1000 h, 280 h, DE00 h,
    102 h, D1B0 h, 800 h,  #10 h, 001 h, 0100 h, 002 h, 0100 h, 003 h, 1030 h,
    009 h, 0001 h, 00C h, 0000 h, 090 h, 8000 h, 00F h, 0000 h, 210 h, 0000 h,
    211 h, 00EF h, 212 h, 0000 h, 213 h, 018F h, 500 h, 0000 h, 501 h, 0000 h,
    502 h, 005F h, 401 h, 0001 h, 404 h, 0000 h, 800 h,  #10 h, 007 h, 0100 h,
    800 h,  #10 h, 200 h, 0000 h, 201 h, 0000 h, 800 h,  #10 h,
    001 h, 0100 h, 003 h, 1030 h, FFF h,
decimal align

: tft@ ( reg -- val )  LCD-REG h! LCD-RAM h@ ;
: tft! ( val reg -- )  LCD-REG h! LCD-RAM h! ;

$0000 variable tft-bg
$FFFF variable tft-fg

: tft-init ( -- )
  tft-fsmc
  0 LCD-REG h!  0 LCD-REG h!  10 ms
  8 0 do 0  LCD-REG h!  loop  10 ms
  tft:R61509U begin
    dup h@ dup $FFF < while  ( addr reg )
    over 2+ h@ swap  ( addr val reg )
    dup $800 = if drop ms else tft! then
  4 + repeat 2drop ;
