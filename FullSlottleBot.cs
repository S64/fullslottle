using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using S64.Bot.Builder.Adapters.Slack;

namespace FullSlottle
{

    class FullSlottleBot : ActivityHandler
    {
        
        // 区切り文字一覧
        private readonly string[] DELIMITERS = {
            " ", // 半角スペース
            "　", // 全角スペース
            "\r\n", // Windows系改行文字
            "\n", // UNIXライクOS系改行文字
        };

        protected override async Task OnMessageActivityAsync(
            ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken
        )
        {
            if (!SlackAdapter.CHANNEL_ID.Equals(turnContext.Activity.ChannelId))
            {
                // S64.Bot.Builder.Adapters.Slack以外を経由した場合は強制終了
                throw new NotImplementedException();
            }

            var data = turnContext.Activity.ChannelData as SlackChannelData;

            if (!turnContext.Activity.Type.Equals(ActivityTypes.Message) || data.IsMention != true || data.IsBot != true)
            {
                // 人以外やメンション等以外を無視
                return;
            }

            // 区切り文字を使って分割する。第2引数の設定により、空白は削除される。
            var parts = turnContext.Activity.Text.Split(DELIMITERS, StringSplitOptions.RemoveEmptyEntries);

            string response;

            if (parts.Length == 2 && parts[0].StartsWith("<@") && parts[1].Equals("--help"))
            {
                // メンションの形式が "@fullslottle --help" っぽい場合
                response = "メンションより後にスペースや改行区切りの文字を与えると、ひとつチョイスして返すよ。\n詳細は github.com/S64/fullslottle をチェック！";
            }
            else
            {
                // メンション位置を見つける
                var firstMentionIndex = Array.FindIndex(parts, (item) => { return item.StartsWith("<@"); });

                if (parts.Length > (firstMentionIndex+1))
                {
                    // メンション位置よりも先にアイテムが無い場合は無視
                    return;
                }

                // メンション位置 + 1から先をシャッフル対象として取り出す
                var items = parts.Skip(firstMentionIndex + 1);

                // シャッフルして1つだけ取得
                response = items.OrderBy((_) => { return Guid.NewGuid(); }).First();
            }

            await turnContext.SendActivityAsync(
                MessageFactory.Text(
                    $"{response} を選んだよ"
                ),
                cancellationToken
            );
        }

    }

}