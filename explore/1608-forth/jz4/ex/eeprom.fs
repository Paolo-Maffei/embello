\ eeprom access

compiletoram? [if]  forgetram  [then]

\ $40022000 constant FLASH
   FLASH $04 + constant FLASH-PECR
   FLASH $0C + constant FLASH-PEKEYR
   FLASH $18 + constant FLASH-SR

$08080000 constant EE-BASE

: ee! ( u a -- )
  begin FLASH-SR %1 and 0= until
  FLASH-PECR @ %1 and if
    $89ABCDEF FLASH-PEKEYR !
    $02030405 FLASH-PEKEYR !
  then
  !
  begin FLASH-SR @ %1 and 0= until
  %1 FLASH-PECR bis!
;

EE-BASE 16 dump

$55667788 EE-BASE 8 + ee!

EE-BASE 16 dump

$11223344 EE-BASE 8 + ee!

EE-BASE 16 dump
