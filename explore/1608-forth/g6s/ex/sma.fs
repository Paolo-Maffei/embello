\ access HC05 Bluetooth module via UART2, for use with SMA solar inverter

compiletoram? [if]  forgetram  [then]

PA0 constant BT-KEY
PA1 constant BT-RESET

[ifndef] uart2-init
include ../../flib/any/ring.fs
include ../../flib/stm32f1/uart2.fs
include ../../flib/stm32f1/uart2-irq.fs
[then]

: uart-s. ( addr len -- )
  0 ?do dup c@ uart-emit 1+ loop drop ;

: uart-cr ( -- ) $0D uart-emit  $0A uart-emit ;

: at-cmd ( addr len -- )
  s" AT" uart-s.  uart-s.  uart-cr ;

: listen
  500 ms
  begin
    uart-irq-key? while
    uart-irq-key emit
  repeat ;

: listen-hex
  500 ms
  begin
    uart-irq-key? while
    uart-irq-key h.2 space
  repeat ;

: long-listen
  5 0 do listen loop ;

: at-show ( addr len -- )
  at-cmd listen ;

: bt-init ( f -- )
  uart-irq-init
  38400 uart-baud
  OMODE-PP BT-KEY   io-mode!  BT-KEY io!
  OMODE-PP BT-RESET io-mode!  BT-RESET ioc! 10 ms
                              BT-RESET ios! 2000 ms ;

: try
  true bt-init
  10 . s" "                      at-show
  11 . s" +VERSION?"             at-show
  12 . s" +ORGL"                 at-show
  13 . s" +NAME=SMA_SOLAR"       at-show
  14 . s" +PSWD=0000"            at-show
  15 . s" +ROLE=1"               at-show
  16 . s" +RMAAD"                at-show
  17 . s" +UART=38400,0,0"       at-show
  18 . s" +CMODE=1"              at-show
  19 . s" +INIT"                 at-show
  20 . s" +STATE?"               at-show
  21 . s" +IAC=9e8b33"           at-show
  22 . s" +CLASS=1F04"           at-show
  23 . s" +INQM=1,1,10"          at-show
  24 . s" +INQ"                  at-show long-listen
  25 . s" +RNAME?80,25,A4EC14"   at-show long-listen
  26 . s" +PAIR=80,25,A4EC14,20" at-show
  27 . s" +FSAD=80,25,A4EC14"    at-show
  28 . s" +ADDR?"                at-show long-listen
  29 . s" +MRAD?"                at-show long-listen
  30 . s" +BIND=80,25,A4EC14"    at-show
  31 . s" +CMODE=0"              at-show
  32 . s" +STATE?"               at-show
\ 33 . s" +RESET"                at-show
  BT-RESET ioc! ;

: try2
  false bt-init
  s" $$$" uart-s.         listen-hex
  s" C"   uart-s. uart-cr listen-hex
  s" F,1" uart-s. uart-cr listen-hex
  BT-RESET ioc! ;

\ 1234 ms try
