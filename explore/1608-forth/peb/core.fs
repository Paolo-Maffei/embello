\ core definitions

\ <<<board>>>
compiletoflash
( core start: ) here hex.

PC0 constant LED1
PC1 constant LED2
PC2 constant LED3
PC3 constant LED4
PC4 constant LED5
PC5 constant LED6
PC6 constant LED7
PC7 constant LED8

: led-init ( -- )
  8 0 do OMODE-OD  i LED1 +  dup ios!  io-mode! loop ;

PB5 constant SPEAKER

: speaker-init ( -- )
  OMODE-PP SPEAKER io-mode! ;
: beep ( -- )
  1000 0 do pb5 iox! 500 us loop ;

PA0 constant KEY-UP
PE2 constant KEY-LEFT
PE3 constant KEY-DOWN
PE4 constant KEY-RIGHT

: key-init ( -- )
  IMODE-PULL KEY-UP     dup ioc!  io-mode!  \ has external pull-up
  IMODE-PULL KEY-LEFT   dup ios!  io-mode!
  IMODE-PULL KEY-DOWN   dup ios!  io-mode!
  IMODE-PULL KEY-RIGHT  dup ios!  io-mode! ;

: key-demo ( -- )
  key-init led-init
  begin
    KEY-UP    io@ LED1 io!
    KEY-LEFT  io@ LED2 io!
    KEY-DOWN  io@ LED3 io!
    KEY-RIGHT io@ LED4 io!
  key? until
  LED1 ios! ;

\ these re-definitions will switch the flash memory driver to SPI2
: +spi +spi2 inline ;
: -spi -spi2 inline ;
: >spi >spi2 inline ;
: spi> spi2> inline ;

: xspi2-init ( -- )  \ configure all the chip select pins
  OMODE-PP PG13  dup ios!  io-mode!  \ spi flash
  OMODE-PP PG14  dup ios!  io-mode!  \ sd card
  spi2-init ;

include ../flib/spi/smem.fs

: smem-demo ( -- )
  PG13 ssel2 ! ." ID: " smem-id hex. ." , " smem-size . ." kB " ;

\ FSMC banks 3 and 4
$A0000010 constant FSMC-BCR3
$A0000014 constant FSMC-BTR3
$A0000018 constant FSMC-BCR4
$A000001C constant FSMC-BTR4

\ SRAM on PZ6806L experimentation board (256Kx16, IS62WV51216BLL-55TLI)

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

include ../flib/mecrisp/quotation.fs
include ../flib/mecrisp/multi.fs
include ../flib/any/timed.fs

: init init led-init speaker-init key-init i2c-init ( smem-init ) sram-init ;

cornerstone <<<core>>>
hello

\ PG13 ssel2 !
\ spi2-init
\ smem-id .
\ smem-size .
