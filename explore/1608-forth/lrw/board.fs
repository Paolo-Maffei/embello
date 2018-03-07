\ board definitions
\ needs always.fs

eraseflash
cr
compiletoflash
( board start: ) here dup hex.

include ../flib/mecrisp/calltrace.fs
include ../flib/mecrisp/cond.fs
include ../flib/mecrisp/hexdump.fs
include ../flib/stm32l0/io.fs
include ../flib/pkg/pins64.fs
include ../flib/stm32l0/hal.fs
include ../flib/stm32l0/adc.fs
include ../flib/stm32l0/timer.fs
include ../flib/stm32l0/pwm.fs
include ../flib/stm32l0/sleep.fs

include ../flib/any/ring.fs
include ../flib/stm32l0/uart2.fs
include ../flib/stm32l0/uart2-irq.fs

: hello ( -- ) flash-kb . ." KB <lrw> " hwid hex.
  $30000 compiletoflash here -  flashvar-here compiletoram here -
  ." ram/flash: " . . ." free " ;

: init ( -- )  \ board initialisation
  init  \ uses new uart init convention
\ ['] ct-irq irq-fault !  \ show call trace in unhandled exceptions
  $00 hex.empty !  \ empty flash shows up as $00 iso $FF on these chips
  16MHz ( set by Mecrisp on startup to get an accurate USART baud rate )
\ 2 RCC-CCIPR !  \ set USART1 clock to HSI16, independent of sysclk
  1000 systick-hz
  hello ." ok." cr
;

( board end, size: ) here dup hex. swap - .
cornerstone <<<board>>>
compiletoram
