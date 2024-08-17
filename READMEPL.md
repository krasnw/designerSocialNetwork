### Idea główna
Ta sieć jest przeznaczona dla grafików, a także osób zainteresowanych zamówieniem usług grafików. Graficy mogą publikować swoje rozwiązania (elementy interfejsu użytkownika) w formie postów. Inni użytkownicy tej sieci mogą oceniać zawartość za pomocą polubień. Na podstawie popularności postów użytkownika tworzona jest jego ranking.  
Tę ocenę graficy mogą wykorzystać podczas rekrutacji w firmach IT, pokazując, że ich rozwiązania przyciągają uwagę ludzi. Ponadto, mając dużą ocenę społecznościową, projektant ma większą szansę, że jego usługi zostaną wykorzystane przez użytkownika zainteresowanego zamówieniem usług.

### Treść umieszczona na platformie 
Platforma powinna zapewniać możliwość komunikacji między klientami i wykonawcami, omawiania szczegółów zamówień i udostępniania plików. 
Użytkownik może przesyłać treści na platformę na różne sposoby, a mianowicie: 
  Dostęp publiczny - otwarty dla wszystkich użytkowników
  Dostęp przez link - dla użytkowników, którzy posiadają link od autora.
  Dostęp prywatny - dla użytkowników, którzy wykupili dostęp za wewnętrzną walutę.

### Waluta wewnętrzna
Platforma posiada wewnętrzną walutę, kupowaną za prawdziwe pieniądze, którą można wykorzystać do:
- Kupowanie dostępu do płatnych treści innych użytkowników przez ograniczony czas
- Tymczasową promocję treści
- Wypłacanie pieniędzy z platformy

### Zamawianie zleceń
Z platformy tej mogą korzystać zarówno graficy (zwani dalej wykonawcą), jak i użytkownicy zainteresowani designem (zwani dalej klientem).
Na tej platformie klient może przeglądać prace grafików, aby wybrać wykonawcę, który zrealizuje jego zamówienie. Płatność jest negocjowalna, odbywa się na platformie za pomocą rubinów

### Referencję
* LinkedIN - jako relacja klient-wykonawca, analogiczna do relacji rekruter-poszukujący pracy.
* LeetCode - jako ocena społecznościowa, którą można pokazać pracodawcom
* UpWork - jako platforma do zamawiania produktów cyfrowych
* Instagram/Behance - jako platforma, na której publikowane są posty/portfolia

### Wymagania funkcjionalne
1. Przechowywanie danych:  
Platforma powinna umożliwiać użytkownikom przesyłanie danych. Te dane to posty zawierające elementy interfejsu użytkownika.
2. Użytkownicy mogą przypisywać poziomy dostępu do swoich postów:
    1. Dostęp publiczny: Treść jest dostępna dla wszystkich użytkowników.
    2. Dostęp przez link: Treść jest dostępna tylko dla użytkowników posiadających specjalny link od autora.
    3. Dostęp prywatny: Treści są dostępne tylko dla użytkowników, którzy wykupili dostęp za pomocą wewnętrznej waluty - rubinów.
3. System rankingowy:  
Użytkownicy mogą oceniać posty innych użytkowników poprzez przyznawanie polubień. Na tej podstawie obliczane są dwie wartości: ranking i popularność. 
    1. Każdy użytkownik może przeglądać ranking innych użytkowników za pomocą wcześniej obliczonej tabeli. Tabela wykorzystuje całkowitą liczbę polubień postów z wybranym tagiem. Jest ona obliczana raz w tygodniu.
    2. Każdy użytkownik może przeglądać swój ranking w każdym z używanych w jego postach tagów.
    3. Każdy użytkownik może przeglądać popularne posty na odpowiedniej stronie. „Popularność” jest obliczana na podstawie stosunku całkowitej liczby polubień do czasu ich otrzymania. Jest ona obliczana raz na godzinę.
4. Wykorzystanie wewnętrznej waluty:  
    1. Użytkownicy mogą kupować rubiny za prawdziwe pieniądze.
    2. Użytkownicy mogą wydawać rubiny na dostęp do postów prywatnych. Dostęp do postów prywatnych jest ograniczony do 30 dni.
    3. Użytkownicy mogą wydawać rubiny na promowanie swoich postów. Promowanie postów zwiększa ich widoczność wśród innych postów.
    4. Użytkownicy mogą wypłacać środki z platformy.
5. System tagów:  
Użytkownik może oznaczać swoje posty gotowymi tagami specjalnymi (np. „Button”, „Glass Morphism”, „Red”). Użytkownik może używać tagów do wyszukiwania treści.
6. Zamawainie zleceń:  
Platforma powinna zapewniać możliwości interakcji między grafikami (wykonawcami) a użytkownikami zainteresowanymi designem (klientami):
    1. Dla klientów: możliwość przeglądania prac grafików, wybierania wykonawców i zamawiania u nich produktów cyfrowych.
    2. Dla artystów: Możliwość dodawania swoich prac, uzyskiwania polubień i ratingów, które mogą być wykorzystane do przyciągnięcia nowych klientów i zwiększenia szans na otrzymanie zamówienia.
7. Możliwości komunikacyjne:  
Platforma powinna zapewniać możliwość komunikacji między klientami i wykonawcami, omawiania szczegółów zamówień i przesyłania plików.
8. Promowanie postów:  
Każdy zarejstrowany użytkownik może promować swoje posty, aby zwiększyć ich widoczność wśród innych postów, za pomocą wewnętrznej waluty - rubinów.
9. Uprawnienia administratora:  
    1. Dodawanie nowych tagów, których mogą używać użytkownicy do kategoryzacji swoich postów.
    2. Wyświetlanie listy skarg dotyczących nieodpowiednich postów.
    3. Blokowanie kont użytkowników.
10. System skarg:  
Każdy zarejestrowany użytkownik może złożyć skargę dotyczącą pojedynczego postu na podstawie wykroczeń wymienionych poniżej. Użytkownik może dodać opis i zrzuty ekranu do skargi. 
    1. Spam
    2. Przemoc
    3. Nieodpowiednia treść do tematyki platformy
    4. Kradzież treści
    5. Inne

### Wymagania pozafunkcjonalne
1. Platforma powinna być w stanie obsłużyć dużą liczbę zgłoszeń treści i miejsc docelowych jednocześnie.
2. Szybki czas reakcji dla użytkowników wchodzących w interakcję z platformą.
3. Platforma powinna być w stanie łatwo skalować się wraz ze wzrostem bazy użytkowników.
4. Obsługiwane powinny być różne typy obrazów.
5. Platforma powinna działać w przeglądarkach Chrome (wersji 120 i wyżej) Safari (wersji 14 i wyżej) FireFox (wersji 122 i wyżej) dla systemów operacyjnych Windows 10 i 11, MacOS 12 i wyżej, Linux.
6. Platforma powinna mieć responsywny interfejs, dla urządzeń mobilnych.
7. Platforma powinna być oparty na długo wspieranych technologiach i rozwiązaniach w celu uniknięcia powtarzających się aktualizacji.
8. Platforma musi działać dla użytkowników 99,9% czasu. Uwzględniając przypadki awaryjne  
9. System powinien gwarantować bezpieczeństwo kont z których korzystają użytkownicy.
10. Platforma powinna tworzyć rating użytkowników, bazując na popularności postów (dostanych Like-ach, czas życia postu).
