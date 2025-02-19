; sobel.asm
; Ustawienie opcji, aby nazwy etykiet by³y traktowane dok³adnie (bez zmiany wielkoœci liter)
option casemap:none
; Oliwier Gebczynski INF sem 5 rok akademicki 2024/25

; Eksportujemy funkcjê, aby by³a widoczna poza DLL-em
PUBLIC AsmSobelFunction

.code

; Funkcja filtru sobela do detekcji krawêdzi obrazu
AsmSobelFunction PROC
    ; ----------------- Prolog -----------------
    ; Zachowujemy rejestry nieulotne zgodnie z Microsoft x64 calling convention
    push    rbp
    mov     rbp, rsp
    push    rbx
    push    rsi
    push    rdi
    push    r12
    push    r13
    push    r14
    push    r15
    ; Rezerwujemy 64 bajty na lokalne zmienne (bufory, akumulatory, itp.)
    sub     rsp, 64         

    ; ----------------- Inicjalizacja parametrów wejœciowych  -----------------
    ; Parametry funkcji (zgodnie z Microsoft x64):
    ; RCX = inputImage, RDX = outputImage, R8 = width, R9 = height
    mov     rsi, rcx        ; rsi wskazuje na wejœciowy obraz (inputImage)
    mov     rdi, rdx        ; rdi wskazuje na wyjœciowy obraz (outputImage)
    mov     rbx, r8         ; rbx zawiera szerokoœæ obrazu (width)
    mov     r10, r9         ; r10 zawiera wysokoœæ obrazu (height)

    ; ----------------- Zerowanie brzegów wyjœciowego obrazu -----------------

    ; Zerujemy górny (pierwszy) wiersz wyjœciowego obrazu
    xor     rax, rax        ; rax = 0 (indeks kolumny)
top_row_loop:
    cmp     rax, rbx        ; sprawdzamy, czy dotarliœmy do koñca wiersza (rax >= width)
    jge     top_row_done
    mov     byte ptr [rdi + rax], 0 ; ustawiamy wartoœæ 0 (czarny piksel)
    inc     rax
    jmp     top_row_loop
top_row_done:

    ; Zerujemy dolny (ostatni) wiersz wyjœciowego obrazu
    ; Obliczamy offset dolnego wiersza: (height - 1) * width
    mov     rax, r10        ; rax = height
    dec     rax             ; rax = height - 1
    imul    rax, rbx        ; rax = (height - 1) * width
    lea     r11, [rdi + rax] ; r11 wskazuje na pocz¹tek dolnego wiersza wyjœciowego obrazu
    xor     rax, rax        ; rax = 0 (indeks kolumny)
bottom_row_loop:
    cmp     rax, rbx        ; iterujemy po wszystkich kolumnach dolnego wiersza
    jge     bottom_row_done
    mov     byte ptr [r11 + rax], 0 ; zerujemy piksel dolnego wiersza (u¿ywamy r11 jako wskaŸnika do dolnego wiersza)
    inc     rax
    jmp     bottom_row_loop
bottom_row_done:

    ; Zerujemy lew¹ kolumnê wyjœciowego obrazu
    xor     rax, rax        ; rax = 0 (indeks wiersza)
left_col_loop:
    cmp     rax, r10        ; sprawdzamy, czy dotarliœmy do koñca obrazu (rax >= height)
    jge     left_col_done
    mov     rcx, rax
    imul    rcx, rbx        ; rcx = rax * width = offset bie¿¹cego wiersza
    mov     byte ptr [rdi + rcx], 0 ; zerujemy piksel w lewej kolumnie bie¿¹cego wiersza
    inc     rax
    jmp     left_col_loop
left_col_done:

    ; Zerujemy praw¹ kolumnê wyjœciowego obrazu
    xor     rax, rax        ; rax = 0 (indeks wiersza)
right_col_loop:
    cmp     rax, r10        ; sprawdzamy, czy dotarliœmy do koñca obrazu
    jge     right_col_done
    mov     rcx, rax
    imul    rcx, rbx        ; rcx = offset bie¿¹cego wiersza
    mov     rdx, rbx        ; rdx = width
    dec     rdx             ; rdx = width - 1 (ostatnia kolumna)
    add     rcx, rdx        ; rcx = offset ostatniego piksela w bie¿¹cym wierszu
    mov     byte ptr [rdi + rcx], 0 ; zerujemy piksel w prawej kolumnie
    inc     rax
    jmp     right_col_loop
right_col_done:

    ; ----------------- Przetwarzanie wnêtrza obrazu -----------------
    ; Przetwarzamy wiersze od 1 do (height - 2), czyli pomijamy brzegowe wiersze
    mov     r11, 1         ; r11 = bie¿¹cy indeks wiersza (rozpoczynamy od 1)
outer_loop:
    mov     rax, r10
    dec     rax             ; rax = height - 1
    cmp     r11, rax
    jge     end_outer_loop ; jeœli r11 >= height - 1, koñczymy pêtlê zewnêtrzn¹

    ; Ustalamy wskaŸniki do trzech kolejnych wierszy wejœciowego obrazu:
    ; - r11 - 1: wiersz powy¿ej bie¿¹cego
    ; - r11    : bie¿¹cy wiersz
    ; - r11 + 1: wiersz poni¿ej bie¿¹cego
    mov     rax, r11
    dec     rax
    imul    rax, rbx      ; obliczamy offset dla wiersza powy¿ej
    add     rax, rsi      ; wskaŸnik do wiersza powy¿ej = inputImage + offset
    mov     qword ptr [rbp - 8], rax  ; zapisujemy wskaŸnik do lokalnej zmiennej (rbp - 8)

    mov     rax, r11
    imul    rax, rbx      ; offset dla bie¿¹cego wiersza
    add     rax, rsi      ; wskaŸnik do bie¿¹cego wiersza = inputImage + offset
    mov     qword ptr [rbp - 16], rax ; zapisujemy wskaŸnik do bie¿¹cego wiersza (rbp - 16)

    mov     rax, r11
    inc     rax
    imul    rax, rbx      ; offset dla wiersza poni¿ej
    add     rax, rsi      ; wskaŸnik do wiersza poni¿ej = inputImage + offset
    mov     qword ptr [rbp - 24], rax ; zapisujemy wskaŸnik do wiersza poni¿ej (rbp - 24)

    ; Ustalamy wskaŸnik do bie¿¹cego wiersza wyjœciowego obrazu
    mov     rax, r11
    imul    rax, rbx
    add     rax, rdi      ; wskaŸnik = outputImage + (r11 * width)
    mov     qword ptr [rbp - 32], rax ; zapisujemy wskaŸnik do bie¿¹cego wiersza wyjœciowego (rbp - 32)

    push    r11           ; zachowujemy bie¿¹cy indeks wiersza na stosie

    ; ----------------- Pêtla wewnêtrzna – przetwarzanie kolumn -----------------
    ; Przetwarzamy kolumny od 1 do (width - 2), pomijaj¹c kolumny brzegowe
    mov     r12, 1        ; r12 = bie¿¹cy indeks kolumny (rozpoczynamy od 1)
inner_loop_start:
    mov     rax, rbx
    dec     rax           ; rax = width - 1
    cmp     r12, rax
    jge     end_inner_loop ; jeœli r12 >= width - 1, koñczymy pêtlê wewnêtrzn¹

    ; Inicjalizujemy akumulatory – zmienne lokalne wykorzystywane do obliczeñ
    ; [rbp - 40] oraz [rbp - 44] s³u¿¹ do akumulacji wartoœci, natomiast [rbp - 48] przechowuje bie¿¹cy indeks kolumny (choæ nie jest dalej wykorzystywany w obliczeniach)
    mov     dword ptr [rbp - 48], r12d
    mov     dword ptr [rbp - 40], 0  ; akumulator 1
    mov     dword ptr [rbp - 44], 0  ; akumulator 2

    ; ----------------- Przetwarzanie piksela w otoczeniu -----------------
    ; Przetwarzamy piksele z trzech wierszy – powy¿ej, bie¿¹cy, poni¿ej – w s¹siedztwie bie¿¹cego piksela (kolumna r12)
    ;
    ; Przetwarzanie wiersza powy¿ej (r11 - 1):
    mov     r13, qword ptr [rbp - 8] ; r13 = wskaŸnik do wiersza powy¿ej
    mov     r14, r12
    dec     r14                   ; r14 = kolumna (r12 - 1) – lewy s¹siad
    movzx   r15d, byte ptr [r13 + r14] ; wczytujemy wartoœæ piksela z lewego s¹siedztwa
    add     dword ptr [rbp - 40], r15d ; dodajemy wartoœæ do akumulatora 1
    add     dword ptr [rbp - 44], r15d ; dodajemy wartoœæ do akumulatora 2

    mov     r13, qword ptr [rbp - 8] ; r13 = wskaŸnik do wiersza powy¿ej
    movzx   r15d, byte ptr [r13 + r12] ; wczytujemy wartoœæ piksela z centralnej pozycji tego wiersza
    add     dword ptr [rbp - 44], r15d ; dodajemy wartoœæ do akumulatora 2
    add     dword ptr [rbp - 44], r15d ; dodajemy jeszcze raz (podwójny wk³ad)

    mov     r13, qword ptr [rbp - 8] ; r13 = wskaŸnik do wiersza powy¿ej
    mov     r14, r12
    inc     r14                   ; r14 = kolumna (r12 + 1) – prawy s¹siad
    movzx   r15d, byte ptr [r13 + r14] ; wczytujemy wartoœæ piksela z prawego s¹siada
    sub     dword ptr [rbp - 40], r15d ; odejmujemy wartoœæ od akumulatora 1
    add     dword ptr [rbp - 44], r15d ; dodajemy wartoœæ do akumulatora 2

    ; Przetwarzanie bie¿¹cego wiersza (r11):
    mov     r13, qword ptr [rbp - 16] ; r13 = wskaŸnik do bie¿¹cego wiersza
    mov     r14, r12
    dec     r14                   ; lewy s¹siad (r12 - 1)
    movzx   r15d, byte ptr [r13 + r14] ; wczytujemy wartoœæ piksela z lewej strony
    add     dword ptr [rbp - 40], r15d ; dodajemy wartoœæ do akumulatora 1 (dwukrotnie)
    add     dword ptr [rbp - 40], r15d

    mov     r13, qword ptr [rbp - 16] ; r13 = wskaŸnik do bie¿¹cego wiersza
    mov     r14, r12
    inc     r14                   ; prawy s¹siad (r12 + 1)
    movzx   r15d, byte ptr [r13 + r14] ; wczytujemy wartoœæ piksela z prawej strony
    sub     dword ptr [rbp - 40], r15d ; odejmujemy wartoœæ od akumulatora 1 (dwukrotnie)
    sub     dword ptr [rbp - 40], r15d

    ; Przetwarzanie wiersza poni¿ej (r11 + 1):
    mov     r13, qword ptr [rbp - 24] ; r13 = wskaŸnik do wiersza poni¿ej
    mov     r14, r12
    dec     r14                   ; lewy s¹siad (r12 - 1)
    movzx   r15d, byte ptr [r13 + r14] ; wczytujemy wartoœæ piksela z lewego s¹siada
    add     dword ptr [rbp - 40], r15d ; dodajemy wartoœæ do akumulatora 1
    sub     dword ptr [rbp - 44], r15d ; odejmujemy wartoœæ od akumulatora 2

    mov     r13, qword ptr [rbp - 24] ; r13 = wskaŸnik do wiersza poni¿ej
    movzx   r15d, byte ptr [r13 + r12] ; wczytujemy wartoœæ piksela z centralnej pozycji wiersza poni¿ej
    sub     dword ptr [rbp - 44], r15d ; odejmujemy wartoœæ od akumulatora 2 (dwukrotnie)
    sub     dword ptr [rbp - 44], r15d

    mov     r13, qword ptr [rbp - 24] ; r13 = wskaŸnik do wiersza poni¿ej
    mov     r14, r12
    inc     r14                   ; prawy s¹siad (r12 + 1)
    movzx   r15d, byte ptr [r13 + r14] ; wczytujemy wartoœæ piksela z prawego s¹siada
    sub     dword ptr [rbp - 40], r15d ; odejmujemy wartoœæ od akumulatora 1
    sub     dword ptr [rbp - 44], r15d ; odejmujemy wartoœæ od akumulatora 2

    ; ----------------- Obliczenie koñcowej wartoœci filtra -----------------
    ; Obliczamy sumê modu³ów obu akumulatorów (tj. |akumulator1| + |akumulator2|)
    mov     eax, dword ptr [rbp - 40] ; przenosimy wartoœæ akumulatora 1 do eax
    cmp     eax, 0
    jge     skip_abs1         ; jeœli akumulator1 jest nieujemny, pomijamy negacjê
    neg     eax               ; w przeciwnym razie zmieniamy znak, aby uzyskaæ wartoœæ bezwzglêdn¹
skip_abs1:
    mov     edx, dword ptr [rbp - 44] ; przenosimy wartoœæ akumulatora 2 do edx
    cmp     edx, 0
    jge     skip_abs2         ; jeœli akumulator2 jest nieujemny, pomijamy negacjê
    neg     edx               ; w przeciwnym razie zmieniamy znak
skip_abs2:
    add     eax, edx          ; suma modu³ów = |akumulator1| + |akumulator2|
    cmp     eax, 255
    jbe     no_clamp          ; jeœli suma <= 255, pozostawiamy j¹ bez zmian
    mov     eax, 255          ; w przeciwnym razie ograniczamy wartoœæ do 255 (maksymalna wartoœæ dla piksela)
no_clamp:
    ; ----------------- Zapis wyniku -----------------
    ; Zapisujemy obliczon¹, ograniczon¹ wartoœæ do odpowiedniego piksela wyjœciowego
    mov     r13, qword ptr [rbp - 32] ; r13 = wskaŸnik do bie¿¹cego wiersza wyjœciowego
    mov     byte ptr [r13 + r12], al  ; zapisujemy wynik (wartoœæ w al) do kolumny r12 w bie¿¹cym wierszu

    ; Przechodzimy do nastêpnej kolumny w bie¿¹cym wierszu
    inc     r12
    jmp     inner_loop_start
end_inner_loop:
    ; Przywracamy zachowany indeks wiersza (pop z stosu) i przechodzimy do nastêpnego wiersza
    pop     r11
    inc     r11
    jmp     outer_loop
end_outer_loop:

    ; ----------------- Epilog -----------------
    ; Przywracamy zaalokowan¹ przestrzeñ lokaln¹ i wszystkie zachowane rejestry
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
