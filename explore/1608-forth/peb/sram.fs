\ SRAM on PZ6806L experimentation board (256Kx16, IS62WV51216BLL-55TLI)

\ uses FSMC bank 3
$A0000010 constant FSMC-BCR3
$A0000014 constant FSMC-BTR3

: sram-pins ( -- )
  8 bit RCC-AHBENR bis!  \ enable FSMC clock
  OMODE-AF-PP OMODE-FAST + dup PD0 %1111111100110011 io-modes!
                           dup PE0 %1111111111000011 io-modes!
                           dup PF0 %1111000000111111 io-modes!
                               PG0 %0001010000111111 io-modes! ;

: sram-fsmc ( -- )
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
\                   \ FSMC_AsynchronousWait_Disable
\                   \ FSMC_ExtendedMode_Disable
\                   \ FSMC_WriteBurst_Disable
  FSMC-BCR3 !

\ for 72 MHz, i.e. 13.89 ns per clock cycle
\ assuming address setup > 70 ns and data setup > 20 ns + 1 cycle
\ started with addr/data/turn as 5/2/1, but even 1/2/0 seems to work fine...
  0
  1 0 lshift or     \ FSMC_AddressSetupTime = 6
\                   \ FSMC_AddressHoldTime = 0
  2 8 lshift or     \ FSMC_DataSetupTime = 3
  0 16 lshift or    \ FSMC_BusTurnAroundDuration = 2
\                   \ FSMC_CLKDivision = 0x00
\                   \ FSMC_DataLatency = 0x00
\                   \ FSMC_AccessMode_A
  FSMC-BTR3 !
  1 FSMC-BCR3 bis!  \ MBKEN:Memorybankenablebit
;

$68000000 constant SRAM

: sram-init ( -- )  \ set up FSMC access to 512 KB SRAM in bank 2
  sram-pins sram-fsmc ;
