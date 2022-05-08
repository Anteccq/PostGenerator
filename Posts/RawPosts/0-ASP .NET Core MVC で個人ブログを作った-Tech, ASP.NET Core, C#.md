ブログを作りました。今まで全くといっていいほど Web に触れてこなかったですが、ふと「ブログでもつくるかぁ」と思い立ったので作ってみました。

今まで Web に触れてこなかった大きな理由は css 辛そう ＆ js 辛そうの２つだったのですが、ASP .NET Core のおかげで js は（ほとんど） C# に置き換えれるし、 css は今まで XAML を触ってきた経験が実は生かせるんじゃね？と思ったりなんかしちゃったりしたという考えの下、半分自分への挑戦課題てきな感覚で取り組んだのでした。

ブログをDBから取り出す機構などなどは未完成で、このポストもソースに埋め込んであるわけですが、とりあえず良い感じにはなったのではないでしょうか。

## 技術について
### フレームワーク
みんな大好き ASP .NET Core MVC を使っています。はい。

なんでってそりゃ C# が好きだからです。はい。
### 利用ライブラリ
このブログで利用させていただいたものは以下です。

#### [Markdig](https://github.com/xoofx/markdig)
説明不要レベルに C# では有名な Markdown パーサですね。
色々と Extensions が充実しているのも強みで、 HTML へ変換する際に class や id を付与することも簡単にできます。

将来的には Markdown でブログを書き、 Markdig を通して HTML 化して DB に保存。
HTML 化されたものを Web アプリ側で取り出して表示するという予定です。

余談ですが、 DI 用にインターフェースを切っておくと、 DB から取り出す機構を作る前でも埋め込みを持ってくるクラスを書いて DI コンテナに登録するだけでよしなに動かせたりと、便利だなと。

SOLID 原則「具体ではなく抽象に依存せよ」の疎結合が、いかに良いことで、大きな恩恵をもたらしてくれているのかがよく分かったきがします。

プロジェクトで単体テストを書く度に実感してはいましたが、改めて感じました。いや、さっさとちゃんとした機構を実装しろという話なんですが。

#### [PrismJs](https://prismjs.com/)
Web に全く触れてこなかったせいで js 系のライブラリは全く分からない状態でしたが、コードブロックのシンタックスハイライトをどうしても良い感じにしたかったために調べて導入しました。

Markdig にもサードパーティの拡張が存在したのですが、 .NET Framework であったり、ハイライトされる部分がいまいちだったりとﾋﾞﾐｮ-----だったので Prism Js のお力を借りました。

Prism さんに頼ったおかげで以下のように綺麗なシンタックスハイライトになりました。

```csharp
public bool Mine(Block block, CancellationToken token = default(CancellationToken))
{
    var target = ToTargetBytes(block.Bits);
    var rnd = new Random();
    var nonceSeed = new byte[sizeof(ulong)];
    rnd.NextBytes(nonceSeed);

    var nonce = BitConverter.ToUInt64(nonceSeed, 0);
    while (!token.IsCancellationRequested)
    {
        block.Nonce = nonce++;
        block.Timestamp = DateTime.UtcNow;
        var hash = block.ComputeId();
        Console.WriteLine(new HexString(hash).ToString());
        if (!HashCheck(hash, target)) continue;
        block.Id = new HexString(hash);

        Console.WriteLine();
        var bytes = JsonSerializer.Serialize(block);
        var json = JsonSerializer.PrettyPrint(bytes);
        Console.WriteLine($"Mined : {json}");
        return true;
    }

    return false;
}
```

## おわりに
せっかく作ったので色々と育てていくつもりです。

ブログも開発もほそぼそとやっていこうと思います。