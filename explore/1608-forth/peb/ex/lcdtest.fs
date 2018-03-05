forgetram

\ clear, putpixel, and display are used by the graphics.fs code

: clear ( -- )
  0 $210 tft!  239 $211 tft!  0 $212 tft!  399 $213 tft!  $202 LCD-REG h!
  tft-bg @  400 240 * 0 do dup LCD-RAM h! loop  drop ;

: putpixel ( x y -- )  \ set a pixel in display memory
  dup $212 tft! $213 tft! dup $210 tft! $211 tft! tft-fg @ $202 tft! ;

: display ( -- ) ;  \ update tft from display memory (ignored)

clear
: a 200 100 do i i putpixel loop ;
a
