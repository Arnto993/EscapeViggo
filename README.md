# Escape Viggo

Escape Viggo er et læringsprosjekt der jeg utforsker hvordan AI-agenter kan brukes til å bygge større og mer komplekse ting enn det man får gjennom en vanlig samtale med en chatbot. Et flerspillerspill gir en konkret ramme for å jobbe med mange deler som må fungere sammen over tid: spillregler, nettverk, brukergrensesnitt, grafikk, lyd og testing.

Prosjektet er ikke ment som dokumentasjon på mine faktiske ferdigheter til å skrive kode selv. Koden utvikles med hjelp av AI-agenter. Min rolle er å se struktur og sammenhenger i kode, definere retning og krav, dele arbeidet i håndterbare oppgaver og vurdere resultatene. Det jeg ønsker å lære og vise, er hvordan jeg kan styre AI-agenter effektivt gjennom et større utviklingsarbeid.

Token-effektivitet er en viktig del av dette: gi agentene relevant kontekst, avgrense hver oppgave og velge kontroller som passer endringen. Målet er å unngå unødvendig lesing, gjentakelser og store ombygginger, samtidig som prosjektet utvikles i små steg som kan testes og følges opp.

## Om spillet

Escape Viggo er en flerspillerprototype for Windows PC, med omtrent 5–8 spillere som mål. Én spiller er Viggo, og de andre er elektrikere på en arbeidsplass. Elektrikerne utfører oppgaver og kan stjele Viggos batteri. Når Viggo oppdager at batteriet er borte, starter Rage og jakten på tyven.

En spilluke går fra mandag til fredag. Rollen som Viggo roterer mellom spillerne, og resultatene samles gjennom uken. Prototypen har elektriske arbeidsoppgaver, avbrytelser, strømbrudd og foreløpig lyd og visuell tilbakemelding. Spillfølelse, balanse og lesbarhet må fortsatt vurderes av mennesker som spiller sammen.

Prosjektet bruker **Unity 6.3 LTS (6000.3.25f1)**, **C#**, **Universal Render Pipeline**, **Netcode for GameObjects** og **Unity Multiplayer Services / Sessions / Relay**.

## Start spillet

### Fra Unity

1. Klon eller last ned prosjektet, og legg prosjektmappen til i Unity Hub.
2. Åpne prosjektet med **Unity 6000.3.25f1**, og vent til import og pakkeoppsett er ferdig.
3. Åpne scenen `Assets/EscapeViggo/Scenes/PrototypeWorksite.unity`.
4. Trykk **Play** for å åpne tilkoblingsmenyen. Bruk fremgangsmåten for lokal spilling eller nettspilling nedenfor.

For å lage en kjørbar Windows-versjon velger du **Escape Viggo > Build > Windows Development Player** i Unity. Deretter starter du `Builds/Windows/EscapeViggo.exe`. Byggmappen er generert lokalt og følger ikke nødvendigvis med når du laster ned kildekoden. Skal du dele spillet, må du dele hele `Builds/Windows`-mappen.

Vanlig oppstart krever ikke at du kjører prosjektets **Setup**-menyer; disse kan bygge om scene- og prefabinnhold.

### Spill lokalt på én PC

Bruk minst to spillvinduer for å prøve samspillet mellom Viggo og en elektriker.

1. Start `EscapeViggo.exe`, velg **Local Development**, og behold IP-adressen **127.0.0.1** og port **7777**.
2. Velg **Guided: 5 min** eller **Quick: 2 min**, og trykk **Start Host**. Verten er Viggo på mandag.
3. Start én eller flere ekstra kopier av spillet. Velg samme IP-adresse og port, og trykk **Start Client**. Disse spillerne starter som elektrikere.
4. Arbeidsdagen starter automatisk etter en kort forberedelsesperiode, så koble til klientene raskt.

Du kan også starte tre spillvinduer automatisk fra PowerShell i prosjektmappen, etter at Windows-versjonen er bygget:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Start-LocalPlaytest.ps1 -Players 3 -Minutes 5
```

Bruk `-Players 2` for to vinduer eller `-Minutes 2` for kortere dager. For å gå raskt gjennom hele uken kan du erstatte `-Minutes 5` med `-DebugWeek`, som gir 30 sekunder per dag. Skriptet starter vanlig, interaktiv spilling.

Lokal spilling krever ingen skytjenester eller konto. På samme lokalnett bruker dere vertens lokale IP-adresse i både verts- og klientmenyen, med samme port. Ikke start flere verter på samme port samtidig.

### Spill over internett

1. Start samme spillversjon på alle PC-ene.
2. Verten velger ønsket dagslengde og trykker **Host Online Game**.
3. Vent på **Connected online**, og del den sekstegnslange **Join Code** med de andre spillerne. Koden kan kopieres via **Esc > Copy join code**.
4. De andre skriver inn koden og trykker **Join Online Game**. IP-adresse og portvideresending er ikke nødvendig.

Nettspilling bruker automatisk anonym Unity-innlogging og prosjektets tilknytning til Unity-tjenestene. På forskjellige PC-er trengs ingen egne profilargumenter. Hvis du starter flere nettkopier på samme Windows-konto, bruk ulike argumenter som `-ev-online-profile host` og `-ev-online-profile friend`, slik at kopiene får separate innloggingsprofiler.

Hvis prosjekttilknytningen mangler eller tilgangen blir avvist, kontroller **Edit > Project Settings > Services** i Unity med en konto som har tilgang til det tilknyttede Unity Cloud-prosjektet. Lokal spilling kan brukes uten dette oppsettet.

Når verten avslutter, avsluttes økten for alle. Spillet overfører ikke vertsrollen til en annen PC. Spilling mellom separate internettforbindelser står fortsatt som en manuell akseptansesjekk.

## Kontroller og spillgang

| Kontroll | Handling |
| --- | --- |
| WASD | Beveg deg |
| Mus | Se deg rundt |
| Shift | Sprint |
| Space | Hopp |
| Hold Ctrl eller C | Huk deg ned |
| E | Samhandle med oppgaver, batteristasjon og andre mål |
| Esc | Slipp musepekeren og åpne menyen |

Etter at du har byttet vindu, velger du **Resume controls** for å styre igjen. Oppgavevinduer bruker musepekeren til du fullfører eller avbryter oppgaven.

Som **elektriker** finner du en arbeidsstasjon, sikter på den og trykker **E**. Følg instruksjonene for kobling, spenningsmåling, stikkontaktbytte eller sikringsmontering. Ved **Battery Station** kan du holde **E** for å stjele batteriet. Bare bæreren ser sin egen bæreindikator; tyveriet starter ikke Rage før Viggo undersøker stasjonen.

Som **Viggo** kan du avbryte arbeid og bruke **POWER CONTROL** til å kutte strømmen. Vanlig bevegelse bruker utholdenhet, og når den er tom, må du hvile. Undersøk batteristasjonen med **E** for å oppdage et tyveri. Under jakten kan du nærme deg en elektriker, sikte og trykke **E** for å forsøke en fangst.

Ved **15:00** stopper arbeidsdagen og resultatene vises. Etter en kort ventetid kan verten trykke **Continue** for neste dag. Oppgaver og dagstilstand nullstilles, Viggo-rollen roterer, og ukepoengene beholdes. Fredag avsluttes med ukens sluttresultat. Bruk **Disconnect / return to start** for å gå tilbake til startmenyen, eller **Quit game** for å avslutte.

## Arbeid med AI-agenter

Arbeidet styres gjennom avgrensede oppgaver, tydelige krav og validering tilpasset risikoen. Små tekst- eller presentasjonsendringer trenger noen få målrettede kontroller; endringer i nettverk eller sentrale spillregler krever mer omfattende testing. Automatiske tester kan kontrollere tilstand og regler, men kan ikke avgjøre om spillet er morsomt eller lett å forstå.

Disse filene gir agentene og meg en felles struktur for arbeidet:

- [AGENTS.md](AGENTS.md): arbeidsregler, avgrensning, tokenbruk og validering.
- [CURRENT_STATE.md](CURRENT_STATE.md): oversikt over prosjektets nåværende tilstand.
- [MILESTONES.md](MILESTONES.md): planlagte leveranser og akseptansekriterier.
- [GAME_SPEC.md](GAME_SPEC.md): spilldesign og ønsket oppførsel.
- [CHARACTER_ARTS_SPECS.md](CHARACTER_ARTS_SPECS.md): spesifikasjoner for figurer og animasjon.
- [ART_DIRECTION_PROPS.md](ART_DIRECTION_PROPS.md): retning for rekvisitter og miljø.
- [Tools/Blender/README.md](Tools/Blender/README.md): oppsett av Blender-arbeidsflyten.
- [MILESTONE_HISTORY.md](MILESTONE_HISTORY.md): historikk ved behov for å undersøke tidligere valg.

Agentene skal lese det som er relevant for oppgaven, bevare fungerende systemer og stoppe når det avtalte arbeidet er ferdig. Dette gjør prosjektet til en praktisk øvelse i å forstå og organisere utvikling med AI-agenter, med bevisst bruk av både kontekst og tokens.
