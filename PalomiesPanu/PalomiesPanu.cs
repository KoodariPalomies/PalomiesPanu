using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Effects;
using Jypeli.Widgets;

/// @author Tuomas Mikko Lintula
/// @version 12.12.2020
/// <summary>
/// Ohjelmointi 1, Harjoitustyö, Palomies Panu 
/// </summary>
public class PalomiesPanu : PhysicsGame
{
    // Luodaan pelaaja ja määritetään sille liikkumisnopeus.
    private PlatformCharacter pelaaja1;
    private readonly double liikkumisnopeus = 300;

    // Määritetään standardikoko oliota varten.
    private const int RUUDUN_KOKO = 40;

    // Luodaan lista olioista, jotka poistetaan kentältä pelin lopussa.
    private readonly List<PhysicsObject> oliot = new List<PhysicsObject>();

    // Pelaajan valitsema sammutin (pelin alussa ei ole valittu sammutinta).
    private string valittuSammutin = "";

    // Sammutettavien kohteiden määrä on ennen pelikentän luomista 0.
    private int sammutettavat = 0;

    // Ladataan kuvat pelin olioita varten.
    private readonly Image pelaajanKuva = LoadImage("panu.png");
    private readonly Image roskispaloKuva = LoadImage("roskispalo.png");
    private readonly Image sammunutroskisKuva = LoadImage("sammunutroskis.png");
    private readonly Image rasvapaloKuva = LoadImage("rasvapalo.png");
    private readonly Image sammunutrasvapaloKuva = LoadImage("sammunutrasvapalo.png");
    private readonly Image rasvarajahdysKuva = LoadImage("rasvaräjähdys.png");
    private readonly Image kasisammutinKuva = LoadImage("käsisammutin.png");
    private readonly Image pikapalopostiKuva = LoadImage("pikapaloposti.png");
    private readonly Image sammutuspeiteKuva = LoadImage("sammutuspeite.png");
    private readonly SoundEffect maaliAani = LoadSoundEffect("maali.wav");


    /// <summary>
    /// Funktio, jossa luodaan pelin aloitustilanne.
    /// </summary>
    public override void Begin()
    {   
        // Luodaan alkuvalikko.
        MultiSelectWindow valikko = new MultiSelectWindow("Tervetuloa Palomies Panun alkusammutuspeliin!",
        "Aloita peli", "Ohjeet", "Lopeta");
        valikko.Color = Color.Azure;
        valikko.BorderColor = Color.Black;
        valikko.AddItemHandler(0, valikko.Destroy);
        valikko.AddItemHandler(1, Ohjeet);
        valikko.AddItemHandler(2, Exit);
        Add(valikko);

        // Luodaan kenttä ja näppäimet pelaajan liikuttamista varten.
        LuoKentta();
        LisaaNappaimet();
        Camera.ZoomFactor = 1.2;
        Camera.StayInLevel = true;

        // Sammutinta ei ole alkutilanteessa valittu.
        valittuSammutin = "";
    }


    /// <summary>
    /// Funktio, joka näyttää ohjeet ja tarinan pelille.
    /// </summary>
    private void Ohjeet()
    {
        // Aloitusvalikon ohjeet -painikkeen takana olevat ohjetekstit.
        SplashScreen start = new SplashScreen("Palomies Panu", 
                                              "Hae pelikentän oikeasta reunasta sopiva sammutin \n" +
                                              "ja ryhdy sammutustöihin.", 
                                              "Tehtävänäsi on sammuttaa paloja.", 
                                              "Palomies Panu on sankari onnistuneiden sammutusten jälkeen.");
        Add(start);
    }


    /// <summary>
    /// Funktio, joka luo kentän.
    /// </summary>
    private void LuoKentta()
    {
        // Pelin kenttä ja oliot luodaan tekstitiedoston mukaisille paikoille.
        // Oliot luodaan omissa aliohjelmissa.
        TileMap kentta = TileMap.FromLevelAsset("kentta1.txt");
        kentta.SetTileMethod('J', LisaaJateastia);
        kentta.SetTileMethod('R', LisaaRasvapalo);
        kentta.SetTileMethod('K', LisaaKasisammutin);
        kentta.SetTileMethod('P', LisaaPikapaloposti);
        kentta.SetTileMethod('S', LisaaSammutuspeite);
        kentta.SetTileMethod('N', LisaaPelaaja);
        kentta.Execute(RUUDUN_KOKO, RUUDUN_KOKO);
        Level.CreateBorders();
        Level.Background.CreateGradient(Color.White, Color.SkyBlue);
    }


    /// <summary>
    /// Lisää sammutettavan kohteen
    /// </summary>
    /// <param name="paikka">Sammutettavan kohteen paikka pelikentällä</param>
    /// <param name="leveys">Sammutettavan kohteen leveys pelikentällä</param>
    /// <param name="korkeus">Sammutettavan kohteen korkeus pelikentällä</param>
    /// <param name="kuva">Sammutettavan kohteen kuva pelikentällä</param>
    /// <param name="tagi">Sammutettavan kohteen tag</param>
    private void LisaaSammutettava(Vector paikka, double leveys, double korkeus, Image kuva, String tagi)
    {
        LisaaKohde(paikka, leveys, korkeus, kuva, tagi);
        sammutettavat++;
    }


    /// <summary>
    /// Lisää kentälle kohteen
    /// </summary>
    /// <param name="paikka">Kohteen paikka pelikentällä</param>
    /// <param name="leveys">Kohteen leveys pelikentällä</param>
    /// <param name="korkeus">Kohteen korkeus pelikentällä</param>
    /// <param name="kuva">Kohteen kuva pelikentällä</param>
    /// <param name="tagi">Kohteen tunniste</param>
    private void LisaaKohde(Vector paikka, double leveys, double korkeus, Image kuva, String tagi)
    {
        PhysicsObject kohde = PhysicsObject.CreateStaticObject(leveys, korkeus);
        kohde.IgnoresCollisionResponse = false;
        kohde.Position = paikka;
        kohde.Image = kuva;
        kohde.Tag = tagi;
        oliot.Add(kohde);
        Add(kohde);
    }


    /// <summary>
    /// Funktio, joka lisää kentälle roskispalon.
    /// </summary>
    /// <param name="paikka">Jäteastian sijainti kentällä</param>
    /// <param name="leveys">Jäteastian korkeus</param>
    /// <param name="korkeus">Jäteastian leveys</param>
    private void LisaaJateastia(Vector paikka, double leveys, double korkeus)
    {
        LisaaSammutettava(paikka, leveys + 10, korkeus + 10, roskispaloKuva, "jateastia");
    }


    /// <summary>
    /// Funktio, joka lisää kentälle rasvapalon.
    /// </summary>
    /// <param name="paikka">Rasvapalon paikka kentällä</param>
    /// <param name="leveys">Rasvapalon leveys</param>
    /// <param name="korkeus">Rasvapalon korkeus</param>
    private void LisaaRasvapalo(Vector paikka, double leveys, double korkeus)
    {
        LisaaSammutettava(paikka, leveys +10, korkeus +10, rasvapaloKuva, "rasvapalo");
    }


    /// <summary>
    /// Funktio, joka lisää kentälle käsisammuttimen.
    /// </summary>
    /// <param name="paikka">Käsisammuttimen paikka kentällä</param>
    /// <param name="leveys">Käsisammuttimen leveys</param>
    /// <param name="korkeus">Käsisammuttimen korkeus</param>
    private void LisaaKasisammutin(Vector paikka, double leveys, double korkeus)
    {
        LisaaKohde(paikka, leveys, korkeus, kasisammutinKuva, "kasisammutin");
    }


    /// <summary>
    /// Funktio, joka lisää kentälle pikapalopostin.
    /// </summary>
    /// <param name="paikka">Pikapalopostin paikka kentällä</param>
    /// <param name="leveys">Pikapalopostin leveys</param>
    /// <param name="korkeus">Pikapalopostin korkeus</param>
    private void LisaaPikapaloposti(Vector paikka, double leveys, double korkeus)
    {
        LisaaKohde(paikka, leveys + 10, korkeus, pikapalopostiKuva, "pikapaloposti");
    }


    /// <summary>
    /// Funktio, joka lisää kentälle sammutuspeitteen.
    /// </summary>
    /// <param name="paikka">Sammutuspeitteen paikka kentällä</param>
    /// <param name="leveys">Sammutuspeitteen leveys</param>
    /// <param name="korkeus">Sammutuspeitteen korkeus</param>
    private void LisaaSammutuspeite(Vector paikka, double leveys, double korkeus)
    {
        LisaaKohde(paikka, leveys, korkeus + 20, sammutuspeiteKuva, "sammutuspeite");
    }


    /// <summary>
    /// Funktio, joka lisää Palomies Panun kentälle.
    /// </summary>
    /// <param name="paikka">Pelaajan aloituspiste</param>
    /// <param name="leveys">Pelaajan leveys</param>
    /// <param name="korkeus">Pelaajan korkeus</param>
    private void LisaaPelaaja(Vector paikka, double leveys, double korkeus)
    {
        // Lisätään pelaaja kentälle
        pelaaja1 = new PlatformCharacter(leveys, korkeus);
        pelaaja1.Position = paikka;
        pelaaja1.Mass = 4.0;
        pelaaja1.Image = pelaajanKuva;

        // Lisätään pelaajalle muihin olioihin osumisesta aiheutuvat toiminnot.
        AddCollisionHandler(pelaaja1, "jateastia", SammuttaaPalon);
        AddCollisionHandler(pelaaja1, "rasvapalo", RasvaRajahdys);
        AddCollisionHandler(pelaaja1, "kasisammutin", Kasisammutin);
        AddCollisionHandler(pelaaja1, "pikapaloposti", Pikapaloposti);
        AddCollisionHandler(pelaaja1, "sammutuspeite", Sammutuspeite);
        Add(pelaaja1);

        // Lisätään oliot listaan.
        oliot.Add(pelaaja1);
    }


    /// <summary>
    /// Funktio, joka lisää käytettävät näppäimet ja ohjeet.
    /// Pelaajaa liikutellaan nuolinäppäimillä.
    /// </summary>
    private void LisaaNappaimet()
    {
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Left, ButtonState.Down, Liikuta, "Pelaaja liikkuu vasemmalle", pelaaja1, new Vector(-liikkumisnopeus, 0));
        Keyboard.Listen(Key.Left, ButtonState.Released, Liikuta, "Pelaaja pysähtyy", pelaaja1, Vector.Zero);
        Keyboard.Listen(Key.Right, ButtonState.Down, Liikuta, "Pelaaja liikkuu oikealle", pelaaja1, new Vector(liikkumisnopeus, 0));
        Keyboard.Listen(Key.Right, ButtonState.Released, Liikuta, "Pelaaja pysähtyy", pelaaja1, Vector.Zero);
        Keyboard.Listen(Key.Down, ButtonState.Down, Liikuta, "Pelaaja liikkuu alaspäin", pelaaja1, new Vector(0, -liikkumisnopeus));
        Keyboard.Listen(Key.Down, ButtonState.Released, Liikuta, "Pelaaja pysähtyy", pelaaja1, Vector.Zero);
        Keyboard.Listen(Key.Up, ButtonState.Down, Liikuta, "Pelaaja liikkuu ylöspäin", pelaaja1, new Vector(0, liikkumisnopeus));
        Keyboard.Listen(Key.Up, ButtonState.Released, Liikuta, "Pelaaja pysähtyy", pelaaja1, Vector.Zero);
    }


    /// <summary>
    /// Funktio, joka liikuttaa Palomies Panua.
    /// </summary>
    /// <param name="pelaaja">Pelaaja, jota liikutetaan.</param>
    /// <param name="suunta">Suunta, johon hahmoa liikutellaan.</param>
    private void Liikuta(PhysicsObject pelaaja, Vector suunta)
    {
        pelaaja.Velocity = suunta;
    }


    /// <summary>
    /// Funktio, joka sammuttaa palavan objektin.
    /// </summary>
    /// <param name="hahmo">Palon sammuttava pelaaja</param>
    /// <param name="jateastia">Sammutettava jäteastia</param>
    private void SammuttaaPalon(PhysicsObject hahmo, PhysicsObject jateastia)
    {
        // Mikäli valittuna on jokin sammuttimista, sammutustyö onnistuu.
        if ((valittuSammutin == "pikapaloposti") || (valittuSammutin == "kasisammutin") || (valittuSammutin == "sammutuspeite"))
        {
            maaliAani.Play();

            // Lisätään tekstikenttä pelaajan kosketuksen jälkeen.
            Label tekstikentta = new Label(300.0, 30.0, "Sammutit palon!");
            tekstikentta.X = Screen.Left + 500;
            tekstikentta.Y = Screen.Top - 300;
            tekstikentta.Color = Color.Azure;
            tekstikentta.TextColor = Color.White;
            tekstikentta.BorderColor = Color.Black;
            Add(tekstikentta);
            tekstikentta.LifetimeLeft = TimeSpan.FromSeconds(5.0);

            // Lisätään savu ja uusi kuva palon sammutuksen jälkeen.
            Smoke savu = new Smoke();
            savu.Position = jateastia.Position;
            Add(savu);
            jateastia.Image = sammunutroskisKuva;
            savu.LifetimeLeft = TimeSpan.FromSeconds(4.0);

            // Sammutettavien määrä vähenee yhdellä ja peli päättyy, 
            // mikäli sammutettavien määrä on nolla (2 --> 1 --> 0 ja peli päättyy).
            sammutettavat--;
            LopetetaankoPeli();
        }
    }


    /// <summary>
    /// Funktio, joka saa aikaan rasvapalon rasvaräjähdyksen.
    /// </summary>
    /// <param name="hahmo">Pelaaja, jota joko sammuttaa palon tai aiheuttaa rasvaräjähdyksen</param>
    /// <param name="rasvapalo">Palo, jota sammutetaan</param>
    private void RasvaRajahdys(PhysicsObject hahmo, PhysicsObject rasvapalo)
    {
        // Mikäli valittuna sammuttimena on pikapaloposti, eli sammutusaineena on vesi,
        // aiheutuu sammutuksesta rasvaräjähdys.
        if (valittuSammutin == "pikapaloposti")
        {
            Level.Background.Image = rasvarajahdysKuva;
            maaliAani.Play();

            // Lisätään tekstikenttä pelaajan kosketuksen jälkeen.
            Label tekstikentta = new Label(500.0, 30.0, "Aiheutit rasvaräjähdyksen ja hävisit pelin!");
            tekstikentta.X = Screen.Left + 500;
            tekstikentta.Y = Screen.Top - 300;
            tekstikentta.Color = Color.Azure;
            tekstikentta.TextColor = Color.White;
            tekstikentta.BorderColor = Color.Black;
            Add(tekstikentta);
            tekstikentta.LifetimeLeft = TimeSpan.FromSeconds(5.0);

            // Kutsutaan aliohjelmaa, joka poistaa oliot kentältä ja aloittaa uuden pelin viiden sekunnin kuluttua.
            Poista(oliot);
            Timer.SingleShot(5, Begin);
        }

        // Mikäli valittu sammutin on muu kuin pikapaloposti, sammutus onnistuu.
        else if ((valittuSammutin == "kasisammutin") || (valittuSammutin == "sammutuspeite"))
        {
            // Lisätään savu ja uusi kuva palon sammutuksen jälkeen.
            Smoke savu = new Smoke();
            savu.Position = rasvapalo.Position;
            Add(savu);
            rasvapalo.Image = sammunutrasvapaloKuva;
            savu.LifetimeLeft = TimeSpan.FromSeconds(4.0);
            maaliAani.Play();

            // Lisätään tekstikenttä pelaajan kosketuksen jälkeen.
            Label tekstikentta = new Label(300.0, 30.0, "Sammutit palon!");
            tekstikentta.X = Screen.Left + 500;
            tekstikentta.Y = Screen.Top - 300;
            tekstikentta.Color = Color.Azure;
            tekstikentta.TextColor = Color.White;
            tekstikentta.BorderColor = Color.Black;
            Add(tekstikentta);
            tekstikentta.LifetimeLeft = TimeSpan.FromSeconds(5.0);
            sammutettavat--;

            // Peliä jatketaan, mikäli sammutettavia paloja vielä on kentällä.
            LopetetaankoPeli();
        }
    }


    /// <summary>
    /// Funktio, joka valitsee ja kertoo käsisammuttimen.
    /// </summary>
    /// <param name="hahmo">Pelaaja, joka valitsee sammuttimen</param>
    /// <param name="kasisammutin">Valittava sammutin</param>
    private void Kasisammutin(PhysicsObject hahmo, PhysicsObject kasisammutin)
    {
        maaliAani.Play();

        // Lisätään tekstikenttä pelaajan kosketuksen jälkeen.
        Label tekstikentta = new Label(200.0, 30.0, "Käsisammutin");
        tekstikentta.X = Screen.Left + 500;
        tekstikentta.Y = Screen.Top - 300;
        tekstikentta.Color = Color.Azure;
        tekstikentta.TextColor = Color.White;
        tekstikentta.BorderColor = Color.Black;
        Add(tekstikentta);
        tekstikentta.LifetimeLeft = TimeSpan.FromSeconds(5.0);

        // Käsisammuttimesta tulee valittu sammutin.
        valittuSammutin = kasisammutin.Tag.ToString();
    }


    /// <summary>
    /// Funktio, joka valitsee ja kertoo pikapalopostin.
    /// </summary>
    /// <param name="hahmo">Pelaaja, joka valitsee sammuttimen</param>
    /// <param name="pikapaloposti">Valittava sammutin</param>
    private void Pikapaloposti(PhysicsObject hahmo, PhysicsObject pikapaloposti)
    {
        maaliAani.Play();

        // Lisätään tekstikenttä pelaajan kosketuksen jälkeen.
        Label tekstikentta = new Label(200.0, 30.0, "Pikapaloposti");
        tekstikentta.X = Screen.Left + 500;
        tekstikentta.Y = Screen.Top - 300;
        tekstikentta.Color = Color.Azure;
        tekstikentta.TextColor = Color.White;
        tekstikentta.BorderColor = Color.Black;
        Add(tekstikentta);
        tekstikentta.LifetimeLeft = TimeSpan.FromSeconds(5.0);

        // Pikapalopostista tulee valittu sammutin.
        valittuSammutin = pikapaloposti.Tag.ToString();
    }


    /// <summary>
    /// Funktio, joka valitsee ja kertoo sammutuspeitteen.
    /// </summary>
    /// <param name="hahmo">Pelaaja, joka valitsee sammuttimen</param>
    /// <param name="sammutuspeite">Valittava sammutin</param>
    private void Sammutuspeite(PhysicsObject hahmo, PhysicsObject sammutuspeite)
    {
        maaliAani.Play();

        // Lisätään tekstikenttä pelaajan kosketuksen jälkeen.
        Label tekstikentta = new Label(200.0, 30.0, "Sammutuspeite");
        tekstikentta.X = Screen.Left + 500;
        tekstikentta.Y = Screen.Top - 300;
        tekstikentta.Color = Color.Azure;
        tekstikentta.TextColor = Color.White;
        tekstikentta.BorderColor = Color.Black;
        Add(tekstikentta);
        tekstikentta.LifetimeLeft = TimeSpan.FromSeconds(5.0);

        // Sammutuspeitteestä tulee valittu sammutin.
        valittuSammutin = sammutuspeite.Tag.ToString();
    }


    /// <summary>
    /// Funktio, joka poistaa oliot pelistä.
    /// </summary>
    /// <param name="oliot">Listalle tulevat oliot, jotka tuhotaan alkusammutuksen epäonnistuessa</param>
    private void Poista(List<PhysicsObject> oliot)
    {
        // Muodostetaan silmukka, jolla tuhotaan kaikki kentälle luodut oliot pelin päättymistä varten.
        for (int i = oliot.Count - 1; i >= 0; i--)
        {
            Explosion rajahdys = new Explosion(50);
            rajahdys.Position = oliot[i].Position;
            Add(rajahdys);
            oliot[i].Destroy();
            oliot.Remove(oliot[i]);
        }
    }


    /// <summary>
    /// Funktio, jolla lopetetaan peli onnistuneen alkusammutusurakan jälkeen.
    /// </summary>
    private void LopetetaankoPeli()
    {
        if (sammutettavat == 0)
        {
            // Poistetaan kentältä kaikki oliot ja lisätään taustakuvaksi Palomies Panun kuva.
            ClearAll();
            Level.Background.Image = pelaajanKuva;

            // Lisätään tekstikenttä pelaajan kosketuksen jälkeen.
            Label tekstikentta = new Label(500.0, 30.0, "Sammutit kaikki palot! Palomies Panu on sankari!");
            tekstikentta.X = Screen.Left + 500;
            tekstikentta.Y = Screen.Top - 300;
            tekstikentta.Color = Color.Azure;
            tekstikentta.TextColor = Color.White;
            tekstikentta.BorderColor = Color.Black;
            Add(tekstikentta);
            tekstikentta.LifetimeLeft = TimeSpan.FromSeconds(5.0);

            // Peli palaa aloitustilanteeseen viiden sekunnin kuluttua.
            Timer.SingleShot(5, Begin);
        }
    }
}
