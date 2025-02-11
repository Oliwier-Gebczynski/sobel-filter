; sobel.asm
; Ustawienie opcji, aby nazwy etykiet by³y traktowane dok³adnie (bez zmiany wielkoœci liter)
option casemap:none

; Eksportujemy funkcjê, aby by³a widoczna poza DLL-em
PUBLIC AsmSobelFunction

.code
AsmSobelFunction PROC
    ; Prolog – zachowujemy rejestry nieulotne zgodnie z Microsoft x64 calling convention
    push    rbp
    mov     rbp, rsp
    push    rbx
    push    rsi
    push    rdi
    push    r12
    push    r13
    push    r14
    push    r15
    sub     rsp, 64         ; zaalokowanie przestrzeni lokalnej

    ; Parametry (zgodnie z Microsoft x64):
    ; RCX = inputImage, RDX = outputImage, R8 = width, R9 = height
    mov     rsi, rcx        ; rsi = inputImage
    mov     rdi, rdx        ; rdi = outputImage
    mov     rbx, r8         ; rbx = width
    mov     r10, r9         ; r10 = height

    ; Zerujemy górny wiersz wyjœciowego obrazu
    xor     rax, rax
top_row_loop:
    cmp     rax, rbx
    jge     top_row_done
    mov     byte ptr [rdi + rax], 0
    inc     rax
    jmp     top_row_loop
top_row_done:

    ; Zerujemy dolny wiersz wyjœciowego obrazu
    mov     rax, r10
    dec     rax
    imul    rax, rbx
    lea     r11, [rdi + rax]
    xor     rax, rax
bottom_row_loop:
    cmp     rax, rbx
    jge     bottom_row_done
    mov     byte ptr [r11 + rax], 0
    inc     rax
    jmp     bottom_row_loop
bottom_row_done:

    ; Zerujemy lew¹ kolumnê wyjœciowego obrazu
    xor     rax, rax
left_col_loop:
    cmp     rax, r10
    jge     left_col_done
    mov     rcx, rax
    imul    rcx, rbx
    mov     byte ptr [rdi + rcx], 0
    inc     rax
    jmp     left_col_loop
left_col_done:

    ; Zerujemy praw¹ kolumnê wyjœciowego obrazu
    xor     rax, rax
right_col_loop:
    cmp     rax, r10
    jge     right_col_done
    mov     rcx, rax
    imul    rcx, rbx
    mov     rdx, rbx
    dec     rdx
    add     rcx, rdx
    mov     byte ptr [rdi + rcx], 0
    inc     rax
    jmp     right_col_loop
right_col_done:

    ; Przetwarzamy piksele wnêtrza obrazu (wiersze 1..height-2)
    mov     r11, 1           ; r11 = bie¿¹cy indeks wiersza (pocz¹wszy od 1)
outer_loop:
    mov     rax, r10
    dec     rax
    cmp     r11, rax
    jge     end_outer_loop

    ; Ustalamy wskaŸniki do trzech kolejnych wierszy wejœciowego obrazu:
    ; [rbp - 8]  : wiersz powy¿ej bie¿¹cego (r11-1)
    ; [rbp - 16] : bie¿¹cy wiersz (r11)
    ; [rbp - 24] : wiersz poni¿ej (r11+1)
    mov     rax, r11
    dec     rax
    imul    rax, rbx
    add     rax, rsi
    mov     qword ptr [rbp - 8], rax

    mov     rax, r11
    imul    rax, rbx
    add     rax, rsi
    mov     qword ptr [rbp - 16], rax

    mov     rax, r11
    inc     rax
    imul    rax, rbx
    add     rax, rsi
    mov     qword ptr [rbp - 24], rax

    ; Ustalamy wskaŸnik do bie¿¹cego wiersza wyjœciowego
    mov     rax, r11
    imul    rax, rbx
    add     rax, rdi
    mov     qword ptr [rbp - 32], rax

    push    r11             ; zachowujemy bie¿¹cy indeks wiersza

    ; Przetwarzamy kolumny wnêtrza (od 1 do width-2)
    mov     r12, 1          ; r12 = bie¿¹cy indeks kolumny (od 1)
inner_loop_start:
    mov     rax, rbx
    dec     rax
    cmp     r12, rax
    jge     end_inner_loop

    ; Inicjalizujemy akumulatory – lokalne zmienne (umieszczone na stosie)
    mov     dword ptr [rbp - 48], r12d
    mov     dword ptr [rbp - 40], 0
    mov     dword ptr [rbp - 44], 0

    ; Przetwarzamy wiersz powy¿ej bie¿¹cego (r11-1)
    mov     r13, qword ptr [rbp - 8]
    mov     r14, r12
    dec     r14
    movzx   r15d, byte ptr [r13 + r14]
    add     dword ptr [rbp - 40], r15d
    add     dword ptr [rbp - 44], r15d

    mov     r13, qword ptr [rbp - 8]
    movzx   r15d, byte ptr [r13 + r12]
    add     dword ptr [rbp - 44], r15d
    add     dword ptr [rbp - 44], r15d

    mov     r13, qword ptr [rbp - 8]
    mov     r14, r12
    inc     r14
    movzx   r15d, byte ptr [r13 + r14]
    sub     dword ptr [rbp - 40], r15d
    add     dword ptr [rbp - 44], r15d

    ; Przetwarzamy bie¿¹cy wiersz (r11)
    mov     r13, qword ptr [rbp - 16]
    mov     r14, r12
    dec     r14
    movzx   r15d, byte ptr [r13 + r14]
    add     dword ptr [rbp - 40], r15d
    add     dword ptr [rbp - 40], r15d

    mov     r13, qword ptr [rbp - 16]
    mov     r14, r12
    inc     r14
    movzx   r15d, byte ptr [r13 + r14]
    sub     dword ptr [rbp - 40], r15d
    sub     dword ptr [rbp - 40], r15d

    ; Przetwarzamy wiersz poni¿ej (r11+1)
    mov     r13, qword ptr [rbp - 24]
    mov     r14, r12
    dec     r14
    movzx   r15d, byte ptr [r13 + r14]
    add     dword ptr [rbp - 40], r15d
    sub     dword ptr [rbp - 44], r15d

    mov     r13, qword ptr [rbp - 24]
    movzx   r15d, byte ptr [r13 + r12]
    sub     dword ptr [rbp - 44], r15d
    sub     dword ptr [rbp - 44], r15d

    mov     r13, qword ptr [rbp - 24]
    mov     r14, r12
    inc     r14
    movzx   r15d, byte ptr [r13 + r14]
    sub     dword ptr [rbp - 40], r15d
    sub     dword ptr [rbp - 44], r15d

    ; Obliczamy sumê modu³ów obu akumulatorów
    mov     eax, dword ptr [rbp - 40]
    cmp     eax, 0
    jge     skip_abs1
    neg     eax
skip_abs1:
    mov     edx, dword ptr [rbp - 44]
    cmp     edx, 0
    jge     skip_abs2
    neg     edx
skip_abs2:
    add     eax, edx
    cmp     eax, 255
    jbe     no_clamp
    mov     eax, 255
no_clamp:
    ; Zapisujemy wynik do wyjœciowego obrazu
    mov     r13, qword ptr [rbp - 32]
    mov     byte ptr [r13 + r12], al

    inc     r12
    jmp     inner_loop_start
end_inner_loop:
    pop     r11
    inc     r11
    jmp     outer_loop
end_outer_loop:

    ; Epilog – przywracamy rejestry i zwalniamy lokaln¹ przestrzeñ
    add     rsp, 64
    pop     r15
    pop     r14
    pop     r13
    pop     r12
    pop     rdi
    pop     rsi
    pop     rbx
    pop     rbp
    ret
AsmSobelFunction ENDP

END
