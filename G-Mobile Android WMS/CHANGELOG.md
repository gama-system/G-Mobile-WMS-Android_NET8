# The Changelog

## (November 02 2023)

**Added: **
- Parsowanie długich kodow kreskowych (powyzej 30 znaków) Code128, parametr w Globalnych BarcodeScanningRemoveSpecialCharacters

## (October 2023)
**Fixed**
-Zmiana działania wpisywania ilości dziesiętnych za pomocą przycisku kropki w kontrolce do wpisywania ilości na pozycji dokumentu

## (September 08 2023)

**Fixed: **
- Poprawiono skanowanie w "Stan magazynowy" - wczesniej bralo z ostatniego otwartego dokumentu elementy skanowania. Teraz jest przypisany "template" czyli skanowanie towaru.
- Poprawiono wyswietlanie listy lokalizacji z poziomu "Stan magazynowy"

## (August 08 2023)
**Added: **
-Dodanie konfiguracji formatów czytanych kodów w managerze skanera

## (July 25 2023)

**Fixed: **
- Tryb domyślny dla dokumentów ZL. Tylko jedna paleta SSCC rowniez na dokumencie PZ.

## (July 20 2023)

**Fixed: **
- Dodanie mozliwosci wyboru trybu 'Bezposrednio' i 'Zbiorka i roznoszenie' dla dokumentów ZL

## (July 07 2023)

**Fixed: **
- Wystepowalo dzielenie przez zero '0'. Dodanie sprawdzenie czy jednostka miary istnieje.


## (July 06 2023)

**Fixed: **
- Sprwadzanie numeru SSCC, palety w calej bazie WMS (wcześniej dotyczyło w obrebie jednego dokumentu). Przyjmowanie na stan dokument PW.

## (June 21 2023)

**Added: ** 
- Zrobiono odczyt dat: data produkcji i data przydatnosci. Zmiana wersji -> 1.66 -> 1.81

## (June 14 2023)

**Fixed: **
- Domyślne daty produkcji i przydatności.

**Added: **
- EXIDLokalizacjaDokumentu na Multipickingu

## (May 10 2023)

**Fixed: **
- Domyslny tryb dla dokumentow ZL - przypisywany z ustawien CurrentSettings - DefaultZLMode

## (April 14 2023)

**Added: **

- Mozliwosc wyłączenia proponowana lokalizacja dla pozycji do dokumentow ZL - Opcja wdrozeniowa - LocationPositionsSuggestedZL

## (April 12 2023)

**Improved: **
- usuwanie pustych dokumentow - teraz rowniez przy Zamykaniu dokumentu

**Added: **
- Ustawienie Globalne BarcodeScanningOrderForce - Wymusza skanowanie kolejnych elementow, ukrywa przycik "Koniec".
	Dotyczy skanowania wiecej niz jednego kodu. Np artykul + SSCC.
	Do tej pory mozna bylo przerwac, zostawic sam kod artykulu i wchodzilo w edycje.

## (March 29 2023)

**Fixed: **

- Wyswietlanie na dokumentach ZL (tryb zbierania, ilosc zlecona - byla zebrana)

## (March 24 2023)

**Improved: **

- Dokumenty ZL (szybsza edycja pozycji) - przy edycji pozycji i przechodzeniu na kolejna pomijamy pobieranie listy pozycji

## (March 09 2023)

**Fixed: **

- DocumentsActivity.cs -> update checking documents possible to take

## (February 21 2023)

**Fixed: **

- Jednostka miary do towaru (stan magazynowy)

## (February 20 2023)

**Added: **

- Dodanie obsługi konfiguracji drukarek etykiet (wersja beta)

## (January 23 2023)

**Fixed: **

- Blad przy otwieraniu pozycji - dodanie SYMBOL do "enumow" (settings)

## (January 23 2023)

**Added: **

- Otwieranie dokumentow ZL z bufora w trybie roznoszenia - dodanie kolorów tekstów: zbieranie, roznoszenie

## (January 13 2023)

**Edit: ** 

- Usuniecie wpisów do debugowania - (zeskanowanej lokalizacji) na dokumentach

## (January 13 2023)

**Fixed: **

1. Podstawianie prawidlowej (zeskanowanej lokalizacji) na dokumentach
2. Pole symbol na widoku pozycji
3. Informator o towarze na liscie pozycji oraz liscie dokumentow

## (December 13 2022)

**Added: **

1. Tworzenie ZL z dokumentu PW z bufora
2. Dodanie do opcji wdrozeniowych - blokowanie lokalizacji z ręki

## (December 06 2022)

**Fixed: **

1. Wylaczenie przycisku SKAN..
2. Potwierdzanie pozycji kodem lokalizacji na dokumentach PW,RW,ZL
3. Informacja o towarze (jego lokalizacji) na liscie pozycji - podwójne klikniecie w pozycje otwiera ją a pojedyncze zaznacza

## (November 30 2022)

**Added: **

- Włączenie/wyłączenie skanowania aparatem. Opcja wdrożeniowa.

## (November 28 2022)

**Added: **

- dodano opcje wylaczajaca blokade paska nawiagacyjnego

## (November 25 2022)

**Fixed: **

- Sprawdzanie grupy lokalizacji przy odczycie kodem kreskowym

## (November 25 2022)

**Fixed: **

- Gdy dokument jest w statusie "Wstrzymany" nie dało sie wejsc w dokument z poziomu skanowania kodu z zamowieniem, ale recznie juz sie dalo wejsc
- Informacja o braku towaru lub wejscie w pozycje po seskanowaniu towaru, naprawa komunikatu "Object reference null" przy skanowaniu prawidlowego kodu kreskowego - lista ZL

## (November 18 2022)

**Fixed: **

- Dodawanie widocznosci dla nowych widoków/tekstow/opisow do istniejacych ustawien (ustawienia dokumentów, widoczność pól)

## (November 17 2022)

**Fixed: **

- Naprawa bledow zwiazanych z odczytywaniem lokalizacji na pozycji i wyswieltanej informacji w postaci bledu
- Odczytywanie dokumentu kodem zamowienia zgodnie z typem okna w ktorym się znajduje

**Added: **

- Dodanie SYMBOL-u na widoku pozycji + konfiguracja wyswietlania
- Dodanie mozliwosci zmiany lokalizacji źródłowej na pozycji (konfiguracja w opcjach wdrozeniowych)
- Obsługa dwuetapowego wydania - dodane podkreslnienie '_' do wyszukiwania dokumentów z numerem zamowienia np _ZS/ZL/22/000241

## (September 21 2022)

**Added: **

- dodawanie lokalizacji do dokuemntów inwentaryzacyjnych (uprawnienie w Desktop -> Operatorzy)
- logowanie bez hasla - dla uzytkownikow bez hasla
- mozliwe uruchomienie ze starsza wersja bazy danych (po wpisaniu hasla serwisowego)

**Fixed: **

- blokowanie usuwania pustych dokumentów IN

## (October 17 2022)

**Updated :**

- Required G-Mobile WMS [Server] version

## (September 19 2022)

**Fixed: **

- Wyswietlanie poprawnej ilosci dni do konca licencji.
- Automation update apk version fixed

## (September 13 2022)

**Updated :**

- Required G-Mobile WMS [Server] version

## (September 07 2022)

**Added :**

- Automatyczna kompletacja po zakonczeniu Multipickingu - zwalnianie kuwet z dokumentów po multipickinku

## (September 05 2022)

**Added :**

- Zamykanie dokumentów WZ po multipickinku - jako opcja wdrozeniowa "Multipicking: Ustaw status zamknięty na dok. WZ"

## (September 01 2022)

**Updated :**

- Required G-Mobile WMS [Server] version

## (August 26 2022)

**Updated :**

- Required G-Mobile WMS [Server] version

## (August 25 2022)

**Updated :**

- version code

## (August 18 2022)

**Updated :**

- Xamarin.Android.Build.Tasks version

## (August 11 2022)

**Updated :**

- Required G-Mobile WMS [Server] version

## (August 05 2022)

**Updated :**

- NrKat field support during Multipicking proccess
- DocumentItemActivity_OneStep_ZLMM.cs -> Update locations ID assignment

## (05.08.2022)

**Fixed :**

- Dodanie magazynu do lokalizacji

## (28.07.2022)

**Updated :**

- Changing the display of Notes by a document in the "BL" register.
  Adding information about the number of items on the document and the name of the first item.

## (27.07.2022)

**Fixed :**

- When we are in the position edition window and we scan the goods that are not in the database or which cannot be added to the document, we do not save the current position, but display the appropriate message

## (July 11 2022)

**Updated :**

- Common\BusinessLogicHelpers\DocumentItems.cs file - update Item.ExNrKat variable

## (June 22 2022)

**Updated :**

- Documents.cs file - delete filter for document date

## (May 18 2022)

**Updated :**

- Android app version from 1.64
- Fast/Qucik Creating MM

## (April 27 2022)

**Updated :**

- Android app version from 1.63
- EditingDisplayElements

## (April 27 2022)

**Updated :**

- Alpha background in basket button

## (April 21 2022)

**Added :**

- Android app version from 1.62
- Suggested Detal on button in Documents WZ
- Configuration of enabling and disabling the Detail button for wz
- Configuration of contractor name and register setting for wz - DETAL
- Button in module creating documents. Adding quick text (Zastosowanie dla LIDER)

## (April 15 2022)

**Added :**

- Comment ListView.ItemClick event

## (March 31 2022)

**Updated :**

- Android app version from 1.61 to 1.62
- Numer Katalogowy
- Application Code Version

## (March 31 2022)

**Updated :**

- Android app version from 1.60 to 1.61
- Numer Katalogowy

## (March 21 2022)

**Updated :**

- Android app version from 1.58 to 1.59
- Checkbox in EditingDocumentsActivity

## (March 19 2022)

**Updated :**

- Android app version from 1.57 to 1.58
- Kuweta in kompletacja after multipicking

## (March 9 2022)

**Updated :**

- Android app version from 1.56 to 1.57
- kodean multipicking 
- Multipicking view
- Kodean and serial number view do not overlap

## (March 9 2022)

**Updated :**

- Android app version from 1.55 to 1.56
- Etykieta w module kompletacja

## (March 9 2022)

**Updated :**

- Android app version from 1.54 to 1.55
- Header and view "ilość zlecona" in documents

## (March 8 2022)

**Updated :**

- Android app version from 1.53 to 1.54
-Blocking wrong statuses on button "zatwierdź" in kompletacja 

## (March 8 2022)

**Updated :**

- Android app version from 1.52 to 1.53
- MULTIPICKING color yellow before last item
- BUTTON "BACK" SET ON HIDE IN MULTIPICKING

## (March 1 2022)

**Updated :**

- Android app version from 1.51 to 1.52
- fix notification in MM  

## (March 1 2022)

**Updated :**

- Android app version from 1.50 to 1.51
- Color suggestion MultiPicking(Lokzalizacja, Towar, Kuweta)

## (February 28 2022)

**Updated :**

- Android app version from 1.49 to 1.50
- fix "parsowanie kodu dokumentu" in DocumentsActivity

## (February 22 2022)

**Added :**

- Debugging information

**Updated :**

- Android app version from 1.48 to 1.49
- DocumentItemActivity_OneStep_ZLMM.cs -> OK_Click() method

## (February 11 2022)

**Updated :**

- Android app version from 1.44 to 1.45
