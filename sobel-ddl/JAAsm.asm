; sobel.asm
; Ustawienie opcji, aby nazwy etykiet by�y traktowane dok�adnie (bez zmiany wielko�ci liter)
option casemap:none
; Oliwier Gebczynski INF sem 5 rok akademicki 2024/25

; Eksportujemy funkcj�, aby by�a widoczna poza DLL-em
PUBLIC AsmSobelFunction

.code

; Funkcja filtru sobela do detekcji kraw�dzi obrazu
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

    ; ----------------- Inicjalizacja parametr�w wej�ciowych  -----------------
    ; Parametry funkcji (zgodnie z Microsoft x64):
    ; RCX = inputImage, RDX = outputImage, R8 = width, R9 = height
    mov     rsi, rcx        ; rsi wskazuje na wej�ciowy obraz (inputImage)
    mov     rdi, rdx        ; rdi wskazuje na wyj�ciowy obraz (outputImage)
    mov     rbx, r8         ; rbx zawiera szeroko�� obrazu (width)
    mov     r10, r9         ; r10 zawiera wysoko�� obrazu (height)

    ; ----------------- Zerowanie brzeg�w wyj�ciowego obrazu -----------------

    ; Zerujemy g�rny (pierwszy) wiersz wyj�ciowego obrazu
    xor     rax, rax        ; rax = 0 (indeks kolumny)
top_row_loop:
    cmp     rax, rbx        ; sprawdzamy, czy dotarli�my do ko�ca wiersza (rax >= width)
    jge     top_row_done
    mov     byte ptr [rdi + rax], 0 ; ustawiamy warto�� 0 (czarny piksel)
    inc     rax
    jmp     top_row_loop
top_row_done:

    ; Zerujemy dolny (ostatni) wiersz wyj�ciowego obrazu
    ; Obliczamy offset dolnego wiersza: (height - 1) * width
    mov     rax, r10        ; rax = height
    dec     rax             ; rax = height - 1
    imul    rax, rbx        ; rax = (height - 1) * width
    lea     r11, [rdi + rax] ; r11 wskazuje na pocz�tek dolnego wiersza wyj�ciowego obrazu
    xor     rax, rax        ; rax = 0 (indeks kolumny)
bottom_row_loop:
    cmp     rax, rbx        ; iterujemy po wszystkich kolumnach dolnego wiersza
    jge     bottom_row_done
    mov     byte ptr [r11 + rax], 0 ; zerujemy piksel dolnego wiersza (u�ywamy r11 jako wska�nika do dolnego wiersza)
    inc     rax
    jmp     bottom_row_loop
bottom_row_done:

    ; Zerujemy lew� kolumn� wyj�ciowego obrazu
    xor     rax, rax        ; rax = 0 (indeks wiersza)
left_col_loop:
    cmp     rax, r10        ; sprawdzamy, czy dotarli�my do ko�ca obrazu (rax >= height)
    jge     left_col_done
    mov     rcx, rax
    imul    rcx, rbx        ; rcx = rax * width = offset bie��cego wiersza
    mov     byte ptr [rdi + rcx], 0 ; zerujemy piksel w lewej kolumnie bie��cego wiersza
    inc     rax
    jmp     left_col_loop
left_col_done:

    ; Zerujemy praw� kolumn� wyj�ciowego obrazu
    xor     rax, rax        ; rax = 0 (indeks wiersza)
right_col_loop:
    cmp     rax, r10        ; sprawdzamy, czy dotarli�my do ko�ca obrazu
    jge     right_col_done
    mov     rcx, rax
    imul    rcx, rbx        ; rcx = offset bie��cego wiersza
    mov     rdx, rbx        ; rdx = width
    dec     rdx             ; rdx = width - 1 (ostatnia kolumna)
    add     rcx, rdx        ; rcx = offset ostatniego piksela w bie��cym wierszu
    mov     byte ptr [rdi + rcx], 0 ; zerujemy piksel w prawej kolumnie
    inc     rax
    jmp     right_col_loop
right_col_done:

    ; ----------------- Przetwarzanie wn�trza obrazu -----------------
    ; Przetwarzamy wiersze od 1 do (height - 2), czyli pomijamy brzegowe wiersze
    mov     r11, 1         ; r11 = bie��cy indeks wiersza (rozpoczynamy od 1)
outer_loop:
    mov     rax, r10
    dec     rax             ; rax = height - 1
    cmp     r11, rax
    jge     end_outer_loop ; je�li r11 >= height - 1, ko�czymy p�tl� zewn�trzn�

    ; Ustalamy wska�niki do trzech kolejnych wierszy wej�ciowego obrazu:
    ; - r11 - 1: wiersz powy�ej bie��cego
    ; - r11    : bie��cy wiersz
    ; - r11 + 1: wiersz poni�ej bie��cego
    mov     rax, r11
    dec     rax
    imul    rax, rbx      ; obliczamy offset dla wiersza powy�ej
    add     rax, rsi      ; wska�nik do wiersza powy�ej = inputImage + offset
    mov     qword ptr [rbp - 8], rax  ; zapisujemy wska�nik do lokalnej zmiennej (rbp - 8)

    mov     rax, r11
    imul    rax, rbx      ; offset dla bie��cego wiersza
    add     rax, rsi      ; wska�nik do bie��cego wiersza = inputImage + offset
    mov     qword ptr [rbp - 16], rax ; zapisujemy wska�nik do bie��cego wiersza (rbp - 16)

    mov     rax, r11
    inc     rax
    imul    rax, rbx      ; offset dla wiersza poni�ej
    add     rax, rsi      ; wska�nik do wiersza poni�ej = inputImage + offset
    mov     qword ptr [rbp - 24], rax ; zapisujemy wska�nik do wiersza poni�ej (rbp - 24)

    ; Ustalamy wska�nik do bie��cego wiersza wyj�ciowego obrazu
    mov     rax, r11
    imul    rax, rbx
    add     rax, rdi      ; wska�nik = outputImage + (r11 * width)
    mov     qword ptr [rbp - 32], rax ; zapisujemy wska�nik do bie��cego wiersza wyj�ciowego (rbp - 32)

    push    r11           ; zachowujemy bie��cy indeks wiersza na stosie

    ; ----------------- P�tla wewn�trzna � przetwarzanie kolumn -----------------
    ; Przetwarzamy kolumny od 1 do (width - 2), pomijaj�c kolumny brzegowe
    mov     r12, 1        ; r12 = bie��cy indeks kolumny (rozpoczynamy od 1)
inner_loop_start:
    mov     rax, rbx
    dec     rax           ; rax = width - 1
    cmp     r12, rax
    jge     end_inner_loop ; je�li r12 >= width - 1, ko�czymy p�tl� wewn�trzn�

    ; Inicjalizujemy akumulatory � zmienne lokalne wykorzystywane do oblicze�
    ; [rbp - 40] oraz [rbp - 44] s�u�� do akumulacji warto�ci, natomiast [rbp - 48] przechowuje bie��cy indeks kolumny (cho� nie jest dalej wykorzystywany w obliczeniach)
    mov     dword ptr [rbp - 48], r12d
    mov     dword ptr [rbp - 40], 0  ; akumulator 1
    mov     dword ptr [rbp - 44], 0  ; akumulator 2

    ; ----------------- Przetwarzanie piksela w otoczeniu -----------------
    ; Przetwarzamy piksele z trzech wierszy � powy�ej, bie��cy, poni�ej � w s�siedztwie bie��cego piksela (kolumna r12)
    ;
    ; Przetwarzanie wiersza powy�ej (r11 - 1):
    mov     r13, qword ptr [rbp - 8] ; r13 = wska�nik do wiersza powy�ej
    mov     r14, r12
    dec     r14                   ; r14 = kolumna (r12 - 1) � lewy s�siad
    movzx   r15d, byte ptr [r13 + r14] ; wczytujemy warto�� piksela z lewego s�siedztwa
    add     dword ptr [rbp - 40], r15d ; dodajemy warto�� do akumulatora 1
    add     dword ptr [rbp - 44], r15d ; dodajemy warto�� do akumulatora 2

    mov     r13, qword ptr [rbp - 8] ; r13 = wska�nik do wiersza powy�ej
    movzx   r15d, byte ptr [r13 + r12] ; wczytujemy warto�� piksela z centralnej pozycji tego wiersza
    add     dword ptr [rbp - 44], r15d ; dodajemy warto�� do akumulatora 2
    add     dword ptr [rbp - 44], r15d ; dodajemy jeszcze raz (podw�jny wk�ad)

    mov     r13, qword ptr [rbp - 8] ; r13 = wska�nik do wiersza powy�ej
    mov     r14, r12
    inc     r14                   ; r14 = kolumna (r12 + 1) � prawy s�siad
    movzx   r15d, byte ptr [r13 + r14] ; wczytujemy warto�� piksela z prawego s�siada
    sub     dword ptr [rbp - 40], r15d ; odejmujemy warto�� od akumulatora 1
    add     dword ptr [rbp - 44], r15d ; dodajemy warto�� do akumulatora 2

    ; Przetwarzanie bie��cego wiersza (r11):
    mov     r13, qword ptr [rbp - 16] ; r13 = wska�nik do bie��cego wiersza
    mov     r14, r12
    dec     r14                   ; lewy s�siad (r12 - 1)
    movzx   r15d, byte ptr [r13 + r14] ; wczytujemy warto�� piksela z lewej strony
    add     dword ptr [rbp - 40], r15d ; dodajemy warto�� do akumulatora 1 (dwukrotnie)
    add     dword ptr [rbp - 40], r15d

    mov     r13, qword ptr [rbp - 16] ; r13 = wska�nik do bie��cego wiersza
    mov     r14, r12
    inc     r14                   ; prawy s�siad (r12 + 1)
    movzx   r15d, byte ptr [r13 + r14] ; wczytujemy warto�� piksela z prawej strony
    sub     dword ptr [rbp - 40], r15d ; odejmujemy warto�� od akumulatora 1 (dwukrotnie)
    sub     dword ptr [rbp - 40], r15d

    ; Przetwarzanie wiersza poni�ej (r11 + 1):
    mov     r13, qword ptr [rbp - 24] ; r13 = wska�nik do wiersza poni�ej
    mov     r14, r12
    dec     r14                   ; lewy s�siad (r12 - 1)
    movzx   r15d, byte ptr [r13 + r14] ; wczytujemy warto�� piksela z lewego s�siada
    add     dword ptr [rbp - 40], r15d ; dodajemy warto�� do akumulatora 1
    sub     dword ptr [rbp - 44], r15d ; odejmujemy warto�� od akumulatora 2

    mov     r13, qword ptr [rbp - 24] ; r13 = wska�nik do wiersza poni�ej
    movzx   r15d, byte ptr [r13 + r12] ; wczytujemy warto�� piksela z centralnej pozycji wiersza poni�ej
    sub     dword ptr [rbp - 44], r15d ; odejmujemy warto�� od akumulatora 2 (dwukrotnie)
    sub     dword ptr [rbp - 44], r15d

    mov     r13, qword ptr [rbp - 24] ; r13 = wska�nik do wiersza poni�ej
    mov     r14, r12
    inc     r14                   ; prawy s�siad (r12 + 1)
    movzx   r15d, byte ptr [r13 + r14] ; wczytujemy warto�� piksela z prawego s�siada
    sub     dword ptr [rbp - 40], r15d ; odejmujemy warto�� od akumulatora 1
    sub     dword ptr [rbp - 44], r15d ; odejmujemy warto�� od akumulatora 2

    ; ----------------- Obliczenie ko�cowej warto�ci filtra -----------------
    ; Obliczamy sum� modu��w obu akumulator�w (tj. |akumulator1| + |akumulator2|)
    mov     eax, dword ptr [rbp - 40] ; przenosimy warto�� akumulatora 1 do eax
    cmp     eax, 0
    jge     skip_abs1         ; je�li akumulator1 jest nieujemny, pomijamy negacj�
    neg     eax               ; w przeciwnym razie zmieniamy znak, aby uzyska� warto�� bezwzgl�dn�
skip_abs1:
    mov     edx, dword ptr [rbp - 44] ; przenosimy warto�� akumulatora 2 do edx
    cmp     edx, 0
    jge     skip_abs2         ; je�li akumulator2 jest nieujemny, pomijamy negacj�
    neg     edx               ; w przeciwnym razie zmieniamy znak
skip_abs2:
    add     eax, edx          ; suma modu��w = |akumulator1| + |akumulator2|
    cmp     eax, 255
    jbe     no_clamp          ; je�li suma <= 255, pozostawiamy j� bez zmian
    mov     eax, 255          ; w przeciwnym razie ograniczamy warto�� do 255 (maksymalna warto�� dla piksela)
no_clamp:
    ; ----------------- Zapis wyniku -----------------
    ; Zapisujemy obliczon�, ograniczon� warto�� do odpowiedniego piksela wyj�ciowego
    mov     r13, qword ptr [rbp - 32] ; r13 = wska�nik do bie��cego wiersza wyj�ciowego
    mov     byte ptr [r13 + r12], al  ; zapisujemy wynik (warto�� w al) do kolumny r12 w bie��cym wierszu

    ; Przechodzimy do nast�pnej kolumny w bie��cym wierszu
    inc     r12
    jmp     inner_loop_start
end_inner_loop:
    ; Przywracamy zachowany indeks wiersza (pop z stosu) i przechodzimy do nast�pnego wiersza
    pop     r11
    inc     r11
    jmp     outer_loop
end_outer_loop:

    ; ----------------- Epilog -----------------
    ; Przywracamy zaalokowan� przestrze� lokaln� i wszystkie zachowane rejestry
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
