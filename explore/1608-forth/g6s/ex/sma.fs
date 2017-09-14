\ access HC05 Bluetooth module via UART2, for use with SMA solar inverter

compiletoram? [if]  forgetram  [then]

[ifndef] uart2-init
include ../../flib/any/ring.fs
include ../../flib/stm32f1/uart2.fs
include ../../flib/stm32f1/uart2-irq.fs
[then]

: listen
  begin
    uart-irq-key? while
    uart-irq-key emit
  repeat ;

uart-irq-init
38400 uart-baud

\ $0A    uart-emit
\ 1000 ms

char A uart-emit
char T uart-emit
char + uart-emit
char V uart-emit
char E uart-emit
char R uart-emit
char S uart-emit
char I uart-emit
char O uart-emit
char N uart-emit
char ? uart-emit
$0D    uart-emit
$0A    uart-emit

100 ms listen
