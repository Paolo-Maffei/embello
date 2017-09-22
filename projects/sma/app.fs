\ SMA 5000TL solar inverter readout gadget, reads via Bluetooth, display on LCD
\ -jcw, 2019-09-19

compiletoram? [if]  forgetram  [then]

\ include sma.fs
\ include sma2.fs
\ include lcd.fs

$40003000 constant IWDG
     IWDG $0 + constant IWDG-KR
     IWDG $4 + constant IWDG-PR
     IWDG $8 + constant IWDG-RLR
     IWDG $C + constant IWDG-SR

\ $40021000 constant RCC
     RCC $24 + constant RCC-CSR

: wdog-inhibit $AAAA IWDG-KR ! ;

: wdog-init ( -- )  \ start the watchdog
  begin IWDG-SR c@ 0= until
  $5555 IWDG-KR !     5 IWDG-PR  !  \ 3.2 .. 13,107.2 ms
  $5555 IWDG-KR !  4095 IWDG-RLR !  \ max cycle time
  $CCCC IWDG-KR ! ;

: wdog? ( -- f )  \ true if IWDG caused a reset (flag is cleared once called)
  RCC-CSR @ 29 bit and 0<>
  24 bit RCC-CSR ! ;

\ ( wdog: ) wdog? .

: lcd-msg ( c -- )  \ display an error code in the lower-left corner
  lcd-2nd lcd-emit s" ? " lcd-s. ;

0 variable power
0 variable yield
0 variable total

: app-init ( -- )
  init-display
  wdog? if
    [char] C lcd-msg
  else
\   lcd-clear s" Hello, world!   " lcd-s.
\   lcd-2nd   s"    So what's up?" lcd-s.
    lcd-clear s" Altijd volop zon" lcd-s.
    lcd-2nd   s" bij Sjaak & Loes" lcd-s.
  then ;

: lcd.n ( a n w -- )  \ show converted number in fixed width on lcd
  over ?do bl lcd-emit loop lcd-s. ;

: #.3 ( d -- d )  # # # [char] . hold ;

: show-values ( -- )
  lcd-clear power @ 0 <#     #S #> 4 lcd.n
            yield @ 0 <# #.3 #S #> 8 lcd.n
            s"  kWh" lcd-s.
  lcd-2nd   s"    W" lcd-s.
            total @ 0 <# #.3 #S #> 8 lcd.n
            s"  MWh" lcd-s. ;

: process
  false bt-init
  smaLogin if
    [char] L lcd-msg
  else
    PB0 ioc!  \ LED on
    smaPower 0 max  power !
    smaYield        yield !
    smaTotal 1000 / total !
    show-values
    PB0 ios!  \ LED off
  then
  BT-RESET ioc! ;

: main ( -- )  \ will do a reset if loop doesn't cycle in under 13.1s
  wdog-init app-init cr
  \ processing normally takes 3s, so the cycle time will be approx 8s
\ wdog-init exit
  begin
    wdog-inhibit
    process
    5000 ms
  again ;

\ this is a way to nullify the watchdog once started:
\   ' wdog-inhibit hook-pause !

: rx-connected? ( -- f )  \ true if RX is connected (and idle)
  IMODE-PULL PA10 io-mode!  PA10 ioc!  10 ms  PA10 io@  PA10 ios!  10 ms
  serial-key? if serial-key drop then  \ flush any input noise
;

\ unattended quits to the interpreter if the RX pin is connected, not floating
\ for use with a turnkey app in flash, i.e. ": init init unattended ... ;"

: unattended ( -- ) rx-connected? if quit then ;

: init init unattended main ;
