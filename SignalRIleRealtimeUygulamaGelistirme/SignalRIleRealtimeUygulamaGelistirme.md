# SignalR İle Realtime Uygulama Geliştirme

## SignalR Nedir?
- Client - Server arasındaki klasik haberleşme yöntemi yapılan request'e karşılık verilen response ilişkisi üzerinden eşzamanlı olarak sağlanmaktadır.

- Bu durum hepimizin yıllarca deneyimlediği gibi bir bekleme süreci yahut sayfanın gidip gelmesiyle sonuçlanmakta ve böylece kullanıcı açısından zamansal maliyetle birlikte deneyim açısından günümüze yakışmayan ilkelliğe alamet etmektedir.

<img src="1.png" width="auto">

- Ülke genelinde yapılan bir seçimin sonuçlarını anlık olarak takip eden bir web yazılımının beslendiği kaynağa gelen verileri dinamik olarak yorumlayıp göstermesi gerekirken burada güncel verileri görebilmesi için kullanıcıdan sayfayı yenilemesini istemek sizce ne kadar modern bir hizmet olacaktır?
<img src="2.png" width="auto">

- Ya da günümüzün imtihanı Covid-19(nam-ı değer Korona Virüs) verilerini yayınladığınız güncel bir web uygulamasının anlık olarak girilen verilerinin grafiksel olarak anında yansıtıldığını tahayyül ederseniz klasik web yaklaşımının(request/response) bu süreçte ne kadar efektif bir rol üstenebildiğini söyleyebilirsiniz...
<img src="3.png" width="auto">

- Günümüz ihtiyaçlarını değerlendirirsek eğer klasik web yaklaşımının tek başına pek yeterli olmadığı ve çözüm olarak farklı kütüphanelere ve hatta protokollere ihtiyacımız olduğu ve olunacağı konusunda kaçınılmak olarak hem fikir olduğumuz kanaatindeyim.

- Misal verilen örneklerde Real Time hizmet verebilecek teknolojiye ihtiyaç olduğu ve HTTP'den farklı olarak TCP protokolünü benimseyen WebSocket altyapılı sistemlerin kullanılması gerektiği ortadadır.
<img src="4.png" width="auto">

## WebSocket Nedir?
- TCP bağlantısı Client - Server arasında çift yönlü mesajlaşmayı sağlayan bir protokoldür.

<img src="5.png" width="auto">

## SignalR Nedir?
- SignalR web uygulamalarına, WebSocket teknolojisini kullanarak Real Time fonksiyonellik kazandıran bir Open Source kütüphanedir.

<img src="6.png" width="auto">

## Yapısal Olarak SignalR
- SignalR altında yatan teknoloji WebSocket'tir.
- Özünde RPC(Remote Procedure Call) mekanizmasını benimsemektedir. RPC sayesinde server, client'ta bulunan herhangi bir metodun tetiklenmesini ve veri transferini sağlayabilmektedir.

- Böylece uygulamalar server'dan sayfa yenilemeksizin data transferini sağlamış olacak ve gerçek zamanlı uygulama davranışı sergilemiş olacaktır. Uygulamanın gerçek zamanlı olması client ile server'ın anlık olarak karşılıklı haberleşmesi anlamına gelmektedir.

<img src="7.png" width="auto">

## SignalR Geçmişi
- Microsoft tarafından 2011 yılında geliştirilmiştir. 2013 yılında Asp.NET mimarisine entegre edilmiştir. Günümüzde Asp.NET Core mimarilerinde rahatlıkla kullanılabilmektedir.

- O yıllarda tüm browser'ların WebSocket protokolünü desteklememesi üzerine SignalR'ın kendi altyapısıyla gelerek client ile server arasındaki haberleşmeyi real time olarka gerçekleştirebiliyor olması bir anda popülerliğine yol açmıştır.
<img src="8.png" width="auto">

## SignalR nasıl çalışır?
- SignalR, 'Hub' ismi verilen merkezi bir yapı üzerinden şekillenmektedir. 
- 'Hub' özünde bir class'tır ve içerisinde tanımlanan bir metoda subscribe olan tüm client'lar ilgili 'Hub' üzerinden iletilen mesajları alacaktırlar.
<img src="9.png" width="auto">q6
<img src="10.png" width="auto">