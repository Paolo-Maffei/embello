\ SMA 5000TL solar inverter readout gadget

compiletoram? [if]  forgetram  [then]

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
  $5555 IWDG-KR !  3749 IWDG-RLR !  \ 12,000 ms
  $CCCC IWDG-KR ! ;

: wdog? ( -- f )  \ true if IWDG caused a reset (flag is cleared once called)
  RCC-CSR @ 29 bit and 0<>
  24 bit RCC-CSR ! ;

\ ( wdog: ) wdog? .

: forever ( -- )  \ will reset if loop doesn't cycle in under 12s
  wdog-init cr
  \ readout normally takes 3s, so the cycle time will be approx 10s
  begin
    wdog-inhibit
    try
    7000 ms
  again ;

\ this is a way to nullify the watchdog once started:
\   ' wdog-inhibit hook-pause !
