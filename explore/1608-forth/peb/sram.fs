\ SRAM on PZ6806L experimentation board (512Kx16, IS62WV51216BLL-55TLI)

\ uses FSMC bank 3
$A0000010 constant FSMC-BCR3
$A0000014 constant FSMC-BTR3

$68000000 constant SRAM

: sram-init ( -- )  \ set up FSMC access to 512 KB SRAM in bank 2
  $80               \ keep reset value
  %01 4 lshift or   \ FSMC_MemoryDataWidth_16b
  1 12 lshift or    \ FSMC_WriteOperation_Enable
  FSMC-BCR3 !
  1 0 lshift        \ FSMC_AddressSetupTime
  2 8 lshift or     \ FSMC_DataSetupTime
  FSMC-BTR3 !
  1 FSMC-BCR3 bis!  \ MBKEN:Memorybankenablebit
;
