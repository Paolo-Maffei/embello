\ driver for 2x16 character LCD

compiletoram? [if]  forgetram  [then]

\ PB0..B7 is the 8-bit LCD character data
PB8  constant LCD-E
PB9  constant LCD-R/W
PB10 constant LCD-RS

: lcd! ( c -- )  \ write character to B0..B7
  GPIO-BASE $400 + GPIO.ODR + ( c addr )
  tuck @ $FF bic or swap ! ;

: lcd-send ( b f -- )
  LCD-RS io!  lcd!  LCD-E ios!  5 us  LCD-E ioc!  50 us ;

: lcd-emit ( c -- )
  1 lcd-send ;

: lcd-clear ( -- ) $01 0 lcd-send 50 ms ;
: lcd-home ( -- ) $02 0 lcd-send ;
: lcd-2nd ( -- ) $C0 0 lcd-send ;

: lcd-s. ( addr len -- )
  0 ?do dup c@ lcd-emit 1+ loop drop ;

: init-display
  jtag-deinit  \ disable JTAG on PB3 PB4 PA15
  OMODE-PP PB0     io-mode!
  OMODE-PP PB1     io-mode!
  OMODE-PP PB2     io-mode!
  OMODE-PP PB3     io-mode!
  OMODE-PP PB4     io-mode!
  OMODE-PP PB5     io-mode!
  OMODE-PP PB6     io-mode!
  OMODE-PP PB7     io-mode!
  OMODE-PP LCD-E   io-mode!  LCD-E   ioc!
  OMODE-PP LCD-R/W io-mode!  LCD-R/W ioc!
  OMODE-PP LCD-RS  io-mode!  LCD-RS  ioc!
  100 ms
  $38 0 lcd-send  \ function set
  $0C 0 lcd-send  \ enable, no cursor
  $06 0 lcd-send  \ auto-increment
;

: lcd-demo
  init-display
  lcd-clear s" Hello, world!"    lcd-s.
  lcd-2nd   s"    So what's up?" lcd-s. ;

\ lcd-demo
