\ OLED display
\ needs core.fs

<<<tvebase>>>

+i2c i2c? i2c.
lcd-init
." lcd-init done" cr

show-logo

: showdigit ( n x -- )
  swap 256 * digits + 64 0 do
    32 0 do
      i $1F xor bit over bit@ if over i + j putpixel then
    loop
    4 +
  loop 2drop ;

: showdot ( x -- )
  64 60 do
    dup 4 + over 1+ do i j putpixel loop
  loop drop ;

: shownum ( u -- )
  clear
  10 /mod 10 /mod 10 /mod
  -4 showdigit 28 showdigit 60 showdot 64 showdigit 96 showdigit
  display ;

: .2 ( n -- )  \ display value with two decimal points
  0 swap 0,01 f* 0,005 d+ 2 f.n ;

: go
  bme-init bme-calib
  begin
    500 ms
    cr
    micros bme-data bme-calc >r >r >r micros swap - . ." µs: " r> r> r>
    dup shownum
    .2 ." °C " .2 ." hPa " .2 ." %RH "
  key? until ;

: roll
  1234
  begin
    dup shownum
    1111 + 10000 mod
  key? until ;
