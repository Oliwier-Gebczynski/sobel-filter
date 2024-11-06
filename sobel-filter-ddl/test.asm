;-------------------------------------------------------------------------
;.586
;INCLUDE C:\masm32\include\windows.inc 

.CODE
;-------------------------------------------------------------------------
; To jest przyk³adowa funkcja. 
;-------------------------------------------------------------------------
;parametry funkcji: RCX RDX R8 R9 stos, 
;lub zmiennoprzec.  XMM0 1 2 3
MyProc1 proc		
add 	rcx, rdx
mov 	rax, rcx
jnc ET1
ror	rcx,1
mul 	rcx
ret
ET1:	
neg 	rax
ret
MyProc1 endp

END 			;no entry point
;-------------------------------------------------------------------------
